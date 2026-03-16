using DentalScheduler.Application.Admin.DTOs;
using DentalScheduler.Application.Interfaces;
using MediatR;

namespace DentalScheduler.Application.Admin.Queries.GetUsersWithRoles;

public record GetUsersWithRolesQuery : IRequest<List<UserWithRoleDto>>;

public class GetUsersWithRolesQueryHandler : IRequestHandler<GetUsersWithRolesQuery, List<UserWithRoleDto>>
{
    private readonly IIdentityService _identityService;

    public GetUsersWithRolesQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<List<UserWithRoleDto>> Handle(GetUsersWithRolesQuery request, CancellationToken cancellationToken)
    {
        return await _identityService.GetAllUsersWithRolesAsync();
    }
}

