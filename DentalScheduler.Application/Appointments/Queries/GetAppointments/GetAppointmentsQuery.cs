using DentalScheduler.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalScheduler.Application.Appointments.Queries.GetAppointments;

public record GetAppointmentsQuery : IRequest<List<AppointmentDto>>;

public class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, List<AppointmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAppointmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AppointmentDto>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _context.Appointments
            .OrderByDescending(a => a.StartAt)
            .ToListAsync(cancellationToken);

        var userIds = appointments
            .Select(a => a.PatientId.ToString())
            .Concat(appointments.Select(a => a.DentistId.ToString()))
            .Distinct()
            .ToList();

        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName })
            .ToListAsync(cancellationToken);

        var userMap = users.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim());

        return appointments
            .Select(a => a.ToDto(
                userMap.GetValueOrDefault(a.PatientId.ToString()),
                userMap.GetValueOrDefault(a.DentistId.ToString())))
            .ToList();
    }
}
