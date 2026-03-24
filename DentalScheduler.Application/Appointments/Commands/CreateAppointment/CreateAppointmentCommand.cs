using DentalScheduler.Application.Interfaces;
using DentalScheduler.Domain.Entities;
using DentalScheduler.Domain.Enums;
using MediatR;

namespace DentalScheduler.Application.Appointments.Commands.CreateAppointment;

public record CreateAppointmentCommand : IRequest<Guid>
{
    public Guid PatientId { get; init; }
    public Guid DentistId { get; init; }
    public DateTime StartAt { get; init; }
    public DateTime EndAt { get; init; }
    public string? Notes { get; init; }
    public string? TreatmentType { get; init; }
}

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateAppointmentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        
        // Prevent doctor from booking with themselves
        if (!string.IsNullOrEmpty(currentUserId) && currentUserId == request.DentistId.ToString())
        {
            throw new InvalidOperationException("A doctor cannot schedule an appointment with themselves.");
        }

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = request.PatientId,
            DentistId = request.DentistId,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Notes = request.Notes,
            TreatmentType = request.TreatmentType
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync(cancellationToken);

        return appointment.Id;
    }
}
