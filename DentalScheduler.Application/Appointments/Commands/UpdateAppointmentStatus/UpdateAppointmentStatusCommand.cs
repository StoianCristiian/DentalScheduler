using DentalScheduler.Application.Interfaces;
using DentalScheduler.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalScheduler.Application.Appointments.Commands.UpdateAppointmentStatus;

public record UpdateAppointmentStatusCommand : IRequest<bool>
{
    public Guid AppointmentId { get; init; }
    public AppointmentStatus Status { get; init; }
}

public class UpdateAppointmentStatusCommandHandler : IRequestHandler<UpdateAppointmentStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateAppointmentStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateAppointmentStatusCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

        if (appointment == null) return false;

        appointment.Status = request.Status;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
