using DentalScheduler.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalScheduler.Application.Appointments.Queries.GetAppointments;

public record GetPatientAppointmentsQuery(Guid PatientId) : IRequest<List<AppointmentDto>>;

public class GetPatientAppointmentsQueryHandler : IRequestHandler<GetPatientAppointmentsQuery, List<AppointmentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAwsS3Service _s3Service;

    public GetPatientAppointmentsQueryHandler(IApplicationDbContext context, IAwsS3Service s3Service)
    {
        _context = context;
        _s3Service = s3Service;
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

        var dentistMap = new Dictionary<string, (string Name, string? ProfilePictureUrl)>();
        foreach (var user in dentists)
        {
            var presignedUrl = await _s3Service.GetPresignedUrlAsync(user.ProfilePictureUrl);
            dentistMap[user.Id] = ($"{user.FirstName} {user.LastName}".Trim(), presignedUrl);
        }

        return appointments
            .Select(a => {
                var dentist = dentistMap.GetValueOrDefault(a.DentistId.ToString());
                return a.ToDto(null, dentist.Name, dentist.ProfilePictureUrl);
            })
            .ToList();
    }
}
