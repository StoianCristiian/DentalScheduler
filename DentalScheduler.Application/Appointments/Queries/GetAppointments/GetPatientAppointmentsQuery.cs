using DentalScheduler.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalScheduler.Application.Appointments.Queries.GetAppointments;

public record GetPatientAppointmentsQuery(Guid PatientId) : IRequest<List<AppointmentDto>>;

public class GetPatientAppointmentsQueryHandler : IRequestHandler<GetPatientAppointmentsQuery, List<AppointmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPatientAppointmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AppointmentDto>> Handle(GetPatientAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _context.Appointments
            .Where(a => a.PatientId == request.PatientId)
            .OrderByDescending(a => a.StartAt)
            .ToListAsync(cancellationToken);

        var dentistIds = appointments.Select(a => a.DentistId.ToString()).Distinct().ToList();

        var dentists = await _context.Users
            .Where(u => dentistIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.ProfilePictureUrl })
            .ToListAsync(cancellationToken);

        var dentistMap = dentists.ToDictionary(u => u.Id, u => new { Name = $"{u.FirstName} {u.LastName}".Trim(), u.ProfilePictureUrl });

        return appointments
            .Select(a => {
                var dentist = dentistMap.GetValueOrDefault(a.DentistId.ToString());
                return a.ToDto(null, dentist?.Name, dentist?.ProfilePictureUrl);
            })
            .ToList();
    }
}
