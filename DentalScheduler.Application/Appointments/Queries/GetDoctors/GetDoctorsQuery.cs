using MediatR;

namespace DentalScheduler.Application.Appointments.Queries.GetDoctors;

public record GetDoctorsQuery : IRequest<List<DoctorDto>>;

public class GetDoctorsQueryHandler : IRequestHandler<GetDoctorsQuery, List<DoctorDto>>
{
    private readonly IDoctorRoleChecker _roleChecker;

    public GetDoctorsQueryHandler(IDoctorRoleChecker roleChecker)
    {
        _roleChecker = roleChecker;
    }

    public async Task<List<DoctorDto>> Handle(GetDoctorsQuery request, CancellationToken cancellationToken)
    {
        return await _roleChecker.GetUsersInRoleAsync("Doctor");
    }
}
