using DentalScheduler.Application.DTOs;
using DentalScheduler.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Issuing;

namespace DentalScheduler.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StripeController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeController> _logger;

    public StripeController(
        IPaymentService paymentService,
        IApplicationDbContext context,
        IConfiguration configuration,
        ILogger<StripeController> logger)
    {
        _paymentService = paymentService;
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint pentru Stripe Webhooks
    /// Primește notificări de la Stripe atunci când se schimbă starea plăților
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> WebhookHandler()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();
        
        try
        {
            var stripeEvent = EventUtility.ParseEvent(json);

            // Obține webhook secret din configuration
            var webhookSecret = _configuration["STRIPE_WEBHOOK_SECRET"] ?? _configuration["Stripe:WebhookSecret"];
            var signature = Request.Headers["Stripe-Signature"];

            if (string.IsNullOrEmpty(webhookSecret))
            {
                _logger.LogError("STRIPE_WEBHOOK_SECRET nu este configurat!");
                return BadRequest("Webhook secret not configured");
            }

            // Verifică autenticitatea webhook-ului
            stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);

            // Procesează evenimentul
            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                await HandlePaymentIntentSucceeded(stripeEvent);
            }
            else if (stripeEvent.Type == "payment_intent.payment_failed")
            {
                await HandlePaymentIntentFailed(stripeEvent);
            }

            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.LogError($"Stripe webhook error: {ex.Message}");
            return BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Webhook processing error: {ex.Message}");
            return BadRequest();
        }
    }

    /// <summary>
    /// Procesează plata reușită - marchează appointment-ul ca plătit
    /// </summary>
    private async Task HandlePaymentIntentSucceeded(Event stripeEvent)
    {
        try
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null)
                return;

            _logger.LogInformation($"Payment succeeded for PaymentIntent: {paymentIntent.Id}");

            // Găsește appointment-ul după PaymentIntentId
            var appointment = _context.Appointments
                .FirstOrDefault(a => a.StripePaymentIntentId == paymentIntent.Id);

            if (appointment == null)
            {
                _logger.LogWarning($"Appointment not found for PaymentIntent: {paymentIntent.Id}");
                return;
            }

            // Marchează ca plătit și confirmă
            appointment.IsPaid = true;
            appointment.Confirm();

            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync(CancellationToken.None);

            _logger.LogInformation($"Appointment {appointment.Id} marked as paid and confirmed");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling payment success: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Procesează plata eșuată
    /// </summary>
    private async Task HandlePaymentIntentFailed(Event stripeEvent)
    {
        try
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null)
                return;

            _logger.LogInformation($"Payment failed for PaymentIntent: {paymentIntent.Id}");

            // Găsește appointment-ul
            var appointment = _context.Appointments
                .FirstOrDefault(a => a.StripePaymentIntentId == paymentIntent.Id);

            if (appointment == null)
            {
                _logger.LogWarning($"Appointment not found for PaymentIntent: {paymentIntent.Id}");
                return;
            }

            // Îl marcează ca anulat
            appointment.Reject();

            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync(CancellationToken.None);

            _logger.LogInformation($"Appointment {appointment.Id} rejected due to failed payment");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling payment failure: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Endpoint pentru verificarea stării unei plăți (optional, pentru frontend)
    /// </summary>
    [HttpGet("payment/{paymentIntentId}")]
    [AllowAnonymous]
    public async Task<ActionResult<PaymentIntentDetails>> GetPaymentStatus(string paymentIntentId)
    {
        try
        {
            var details = await _paymentService.GetPaymentIntentAsync(paymentIntentId);
            if (details == null)
                return NotFound();

            return Ok(details);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching payment status: {ex.Message}");
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Inițializează o plată pentru o programare existentă.
    /// Dacă nu există PaymentIntent, îl creează. Dacă există, îl returnează.
    /// </summary>
    [HttpPost("init-payment/{appointmentId:guid}")]
    public async Task<ActionResult<PaymentInitResponse>> InitPayment(Guid appointmentId)
    {
        var appointment = await _context.Appointments.FindAsync(appointmentId);
        
        if (appointment == null)
            return NotFound("Programarea nu a fost găsită.");

        if (appointment.Cost == null || appointment.Cost <= 0)
            return BadRequest("Programarea nu are un cost setat sau costul este 0.");

        string clientSecret = "";
        string paymentIntentId = "";

        try
        {
            // Dacă există deja un intent, îl refolosim
            if (!string.IsNullOrEmpty(appointment.StripePaymentIntentId))
            {
                var intent = await _paymentService.GetPaymentIntentAsync(appointment.StripePaymentIntentId);
                if (intent != null && intent.Status != "succeeded" && intent.Status != "canceled")
                {
                    clientSecret = intent.ClientSecret;
                    paymentIntentId = intent.Id;
                }
            }

            // Dacă nu am găsit un intent valid, creăm unul nou
            if (string.IsNullOrEmpty(clientSecret))
            {
                paymentIntentId = await _paymentService.CreatePaymentIntentAsync(
                    appointment.Cost.Value,
                    appointment.Id.ToString(),
                    $"Plată programare pentru: {appointment.TreatmentType}"
                );
                
                var intent = await _paymentService.GetPaymentIntentAsync(paymentIntentId);
                clientSecret = intent?.ClientSecret ?? throw new InvalidOperationException("Nu s-a putut obține ClientSecret.");

                // Salvăm noul ID în baza de date
                appointment.StripePaymentIntentId = paymentIntentId;
                _context.Appointments.Update(appointment);
                await _context.SaveChangesAsync(CancellationToken.None);
            }

            var publishableKey = _configuration["STRIPE_PUBLISHABLEKEY"] ?? _configuration["Stripe:PublishableKey"];
            
            return Ok(new PaymentInitResponse(publishableKey, clientSecret, paymentIntentId));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Eroare la inițializarea plății: {ex.Message}");
            return BadRequest($"Eroare la plată: {ex.Message}");
        }
    }

    /// <summary>
    /// Sincronizează manual statusul plății între Stripe și baza de date.
    /// Util dacă webhook-ul întârzie sau nu funcționează (ex: localhost).
    /// </summary>
    [HttpPost("sync-status/{appointmentId:guid}")]
    public async Task<IActionResult> SyncPaymentStatus(Guid appointmentId)
    {
        try
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
                return NotFound("Programarea nu a fost găsită.");

            if (string.IsNullOrEmpty(appointment.StripePaymentIntentId))
                return BadRequest("Nu există o plată inițiată pentru această programare.");

            // Verificăm statusul real la Stripe
            var paymentDetails = await _paymentService.GetPaymentIntentAsync(appointment.StripePaymentIntentId);
            
            if (paymentDetails != null && paymentDetails.Status == "succeeded")
            {
                if (!appointment.IsPaid)
                {
                    appointment.IsPaid = true;
                    // Dacă programarea nu era confirmată, o confirmăm acum
                    if (appointment.Status != Domain.Enums.AppointmentStatus.Confirmed && 
                        appointment.Status != Domain.Enums.AppointmentStatus.Completed)
                    {
                        appointment.Confirm();
                    }
                    
                    _context.Appointments.Update(appointment);
                    await _context.SaveChangesAsync(CancellationToken.None);
                    _logger.LogInformation($"Appointment {appointmentId} synced and confirmed via manual check.");
                }
                return Ok(new { status = "Paid", confirmed = true });
            }

            return Ok(new { status = paymentDetails?.Status ?? "Unknown", confirmed = false });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Eroare la sincronizarea plății: {ex.Message}");
            return BadRequest(ex.Message);
        }
    }
}
