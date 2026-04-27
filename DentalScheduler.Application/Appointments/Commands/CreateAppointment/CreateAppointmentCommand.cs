using DentalScheduler.Application.Interfaces;
using DentalScheduler.Domain.Entities;
using DentalScheduler.Domain.Enums;
using MediatR;

namespace DentalScheduler.Application.Appointments.Commands.CreateAppointment;

public record CreateAppointmentCommand : IRequest<CreateAppointmentResponse>
{
    public Guid PatientId { get; init; }
    public Guid DentistId { get; init; }
    public DateTime StartAt { get; init; }
    public DateTime EndAt { get; init; }
    public string? Notes { get; init; }
    public string? TreatmentType { get; init; }
    public decimal Cost { get; init; } // Adaug costul pentru plată
}

public record CreateAppointmentResponse(Guid AppointmentId, string? PaymentIntentClientSecret);

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, CreateAppointmentResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentService _paymentService;

    public CreateAppointmentCommandHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUserService,
        IPaymentService paymentService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _paymentService = paymentService;
    }

    public async Task<CreateAppointmentResponse> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        
        // Prevent doctor from booking with themselves
        if (!string.IsNullOrEmpty(currentUserId) && currentUserId == request.DentistId.ToString())
        {
            throw new InvalidOperationException("A doctor cannot schedule an appointment with themselves.");
        }

        var appointmentId = Guid.NewGuid();
        
        // Nu forța conversia la UTC dacă front-end-ul așteaptă formatul nativ trimis
        var startAtUtc = request.StartAt;
        var endAtUtc = request.EndAt;
        
        // Creează PaymentIntent pe Stripe DOAR dacă avem un cost valid
        string? paymentIntentId = null;
        string? clientSecret = null;
        
        if (request.Cost > 0)
        {
            try
            {
                paymentIntentId = await _paymentService.CreatePaymentIntentAsync(
                    request.Cost,
                    appointmentId.ToString(),
                    $"Appointment for treatment: {request.TreatmentType}"
                );
                
                // Obține detaliile PaymentIntent pentru client secret
                var paymentDetails = await _paymentService.GetPaymentIntentAsync(paymentIntentId);
                clientSecret = paymentDetails?.ClientSecret;
            }
            catch (Exception ex)
            {
                // Putem loga eroarea, dar nu ar trebui să oprească crearea programării dacă plata e opțională la acest pas
                // Totuși, dacă avem cost, ne așteptăm să meargă plata. Pentru moment, aruncăm excepție pentru debugging clar.
                throw new InvalidOperationException($"Failed to create payment intent: {ex.Message}", ex);
            }
        }

        var appointment = new Appointment
        {
            Id = appointmentId,
            PatientId = request.PatientId,
            DentistId = request.DentistId,
            StartAt = startAtUtc,
            EndAt = endAtUtc,
            Notes = request.Notes,
            TreatmentType = request.TreatmentType,
            Cost = request.Cost > 0 ? request.Cost : null, // Salvăm costul doar dacă e setat
            StripePaymentIntentId = paymentIntentId,
            IsPaid = false
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateAppointmentResponse(appointmentId, clientSecret);
    }
}
