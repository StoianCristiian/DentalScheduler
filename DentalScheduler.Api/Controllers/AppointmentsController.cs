using DentalScheduler.Application.Appointments.Commands.CreateAppointment;
using DentalScheduler.Application.Appointments.Queries.GetAppointments;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DentalScheduler.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppointmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateAppointmentCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(id);
    }
    
    [HttpGet]
    public async Task<ActionResult<List<AppointmentDto>>> GetAll()
    {
        return await _mediator.Send(new GetAppointmentsQuery());
    }
}
