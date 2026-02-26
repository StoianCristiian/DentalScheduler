using DentalScheduler.Domain.Entities;
using MediatR;

namespace DentalScheduler.Application.Appointments.Commands.CreateAppointment;

public record CreateAppointmentCommand : IRequest<Guid>
{
    public Guid PatientId { get; init; }
    public Guid DentistId { get; init; }
    public DateTime StartAt { get; init; }
    public DateTime EndAt { get; init; }
    public string? Notes { get; init; }
}

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, Guid>
{
    private readonly Interfaces.IApplicationDbContext _context;

    public CreateAppointmentCommandHandler(Interfaces.IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var entity = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = request.PatientId,
            DentistId = request.DentistId,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Notes = request.Notes
        };

        _context.Appointments.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
