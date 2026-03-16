using DentalScheduler.Application.Interfaces;
using MediatR;

namespace DentalScheduler.Application.Admin.Commands.UpdateUserRole;

public record UpdateUserRoleCommand(string UserId, string NewRole) : IRequest<bool>;

public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, bool>
{
    private readonly IIdentityService _identityService;

    public UpdateUserRoleCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<bool> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        if (!await _identityService.RoleExistsAsync(request.NewRole))
            return false;

        return await _identityService.UpdateUserRoleAsync(request.UserId, request.NewRole);
    }
}
