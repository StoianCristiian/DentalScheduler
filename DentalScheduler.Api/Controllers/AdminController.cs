using DentalScheduler.Application.Admin.Commands.UpdateUserRole;
using DentalScheduler.Application.Admin.DTOs;
using DentalScheduler.Application.Admin.Queries.GetDashboardStats;
using DentalScheduler.Application.Admin.Queries.GetUsersWithRoles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalScheduler.Api.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<AdminDashboardStatsDto>> GetStats()
    {
        return Ok(await _mediator.Send(new GetDashboardStatsQuery()));
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<UserWithRoleDto>>> GetUsers()
    {
        return Ok(await _mediator.Send(new GetUsersWithRolesQuery()));
    }
    
    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(string id, [FromBody] UpdateRoleRequest request)
    {
        var result = await _mediator.Send(new UpdateUserRoleCommand(id, request.NewRole));
        if (!result)
            return BadRequest("Actualizarea rolului a eșuat. Verifică dacă utilizatorul și rolul există.");
            
        return Ok(new { message = "Rol actualizat cu succes." });
    }
}
