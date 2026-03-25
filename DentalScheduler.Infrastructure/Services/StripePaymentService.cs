using DentalScheduler.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace DentalScheduler.Infrastructure.Services;

public class StripePaymentService : IPaymentService
{
    private readonly IConfiguration _configuration;

    public StripePaymentService(IConfiguration configuration)
    {
        _configuration = configuration;
        
        // Setează secret key-ul Stripe din configuration
        var secretKey = _configuration["STRIPE_SECRETKEY"] ?? _configuration["Stripe:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("STRIPE_SECRETKEY nu este configurat!");
        
        StripeConfiguration.ApiKey = secretKey;
    }

    public async Task<string> CreatePaymentIntentAsync(decimal amount, string appointmentId, string description = "")
    {
        try
        {
            // Convertim suma din RON în cenți (Stripe funcționează cu unități mici)
            var amountInCents = (long)(amount * 100);

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInCents,
                Currency = "ron",
                Description = string.IsNullOrEmpty(description) ? $"Appointment #{appointmentId}" : description,
                Metadata = new Dictionary<string, string>
                {
                    { "appointmentId", appointmentId }
                },
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return paymentIntent.Id;
        }
        catch (StripeException ex)
        {
            throw new InvalidOperationException($"Eroare Stripe: {ex.Message}", ex);
        }
    }

    public async Task<PaymentIntentDetails?> GetPaymentIntentAsync(string paymentIntentId)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId);

            if (paymentIntent == null)
                return null;

            return new PaymentIntentDetails
            {
                Id = paymentIntent.Id,
                Amount = paymentIntent.Amount, // Amount is long, not nullable
                Currency = paymentIntent.Currency ?? string.Empty,
                Status = paymentIntent.Status ?? string.Empty,
                Description = paymentIntent.Description,
                ClientSecret = paymentIntent.ClientSecret
            };
        }
        catch (StripeException ex)
        {
            throw new InvalidOperationException($"Eroare Stripe: {ex.Message}", ex);
        }
    }

    public async Task<bool> ConfirmPaymentAsync(string paymentIntentId)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId);

            return paymentIntent?.Status == "succeeded";
        }
        catch (StripeException ex)
        {
            throw new InvalidOperationException($"Eroare Stripe: {ex.Message}", ex);
        }
    }
}
