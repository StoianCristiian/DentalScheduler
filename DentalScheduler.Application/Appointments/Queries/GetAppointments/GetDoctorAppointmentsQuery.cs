using DentalScheduler.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalScheduler.Application.Appointments.Queries.GetAppointments;

public record GetDoctorAppointmentsQuery(Guid DoctorId, DateTime? Date = null) : IRequest<List<AppointmentDto>>;

public class GetDoctorAppointmentsQueryHandler : IRequestHandler<GetDoctorAppointmentsQuery, List<AppointmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDoctorAppointmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AppointmentDto>> Handle(GetDoctorAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Appointments
            .Where(a => a.DentistId == request.DoctorId);

        // Dacă avem o dată specificată, luăm programările din acea zi SAU cele în așteptare (indiferent de dată)
        if (request.Date.HasValue)
        {
            var date = request.Date.Value.Date;
            query = query.Where(a => a.StartAt.Date == date || a.Status == Domain.Enums.AppointmentStatus.Pending);
        }

        var appointments = await query
            .OrderByDescending(a => a.StartAt)
            .ToListAsync(cancellationToken);

        var patientIds = appointments.Select(a => a.PatientId.ToString()).Distinct().ToList();

        var patients = await _context.Users
            .Where(u => patientIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName })
            .ToListAsync(cancellationToken);

        var patientMap = patients.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim());

        return appointments
            .Select(a => a.ToDto(patientMap.GetValueOrDefault(a.PatientId.ToString())))
            .ToList();
    }
}
