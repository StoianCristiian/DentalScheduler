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

    public CreateAppointmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = request.PatientId,
            DentistId = request.DentistId,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Notes = request.Notes,
            TreatmentType = request.TreatmentType,
            Status = AppointmentStatus.Pending
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync(cancellationToken);

        return appointment.Id;
    }
}
