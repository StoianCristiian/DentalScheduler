using DentalScheduler.Application.Appointments.Commands.CreateAppointment;
using DentalScheduler.Application.Appointments.Commands.UpdateAppointmentStatus;
using DentalScheduler.Application.Appointments.Queries.GetAppointments;
using DentalScheduler.Application.Appointments.Queries.GetScheduleRecommendations;
using DentalScheduler.Application.DTOs.AI;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalScheduler.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppointmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET api/appointments — all (admin use)
    [HttpGet]
    public async Task<ActionResult<List<AppointmentDto>>> GetAll()
        => await _mediator.Send(new GetAppointmentsQuery());

    // GET api/appointments/patient/{patientId}
    [HttpGet("patient/{patientId:guid}")]
    public async Task<ActionResult<List<AppointmentDto>>> GetByPatient(Guid patientId)
        => await _mediator.Send(new GetPatientAppointmentsQuery(patientId));

    // GET api/appointments/doctor/{doctorId}
    [HttpGet("doctor/{doctorId:guid}")]
    public async Task<ActionResult<List<AppointmentDto>>> GetByDoctor(Guid doctorId, [FromQuery] DateTime? date)
        => await _mediator.Send(new GetDoctorAppointmentsQuery(doctorId, date));

    // POST api/appointments
    [HttpPost]
    public async Task<ActionResult<CreateAppointmentResponse>> Create([FromBody] CreateAppointmentCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    // PUT api/appointments/{id}/status
    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        var success = await _mediator.Send(new UpdateAppointmentStatusCommand
        {
            AppointmentId = id,
            Status = request.Status,
            Cost = request.Cost
        });

        if (!success)
            return BadRequest("Failed to update status.");

        return NoContent();
    }

    // GET api/appointments/recommend
    [HttpGet("recommend")]
    public async Task<ActionResult<SchedulingResponseDto>> GetRecommendations([FromQuery] GetScheduleRecommendationsQuery query)
    {
        var response = await _mediator.Send(query);
        return Ok(response);
    }
}
