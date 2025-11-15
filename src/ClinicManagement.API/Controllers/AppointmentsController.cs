using ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;
using ClinicManagement.Application.Features.Appointments.Commands.UpdateAppointment;
using ClinicManagement.Application.Features.Appointments.Queries.GetAppointmentById;
using ClinicManagement.Application.Features.Appointments.Queries.GetAppointments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

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

    [HttpGet]
    public async Task<IActionResult> GetAppointments([FromQuery] GetAppointmentsQuery query)
    {
        var result = await _mediator.Send(query);
        
        if (result.Success)
        {
            return Ok(result.Value);
        }
        
        return BadRequest(result.Message);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppointmentById(int id)
    {
        var query = new GetAppointmentByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (result.Success)
        {
            return Ok(result.Value);
        }
        
        return NotFound(result.Message);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAppointment(CreateAppointmentCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (result.Success)
        {
            return CreatedAtAction(nameof(GetAppointmentById), new { id = result.Value?.Id }, result.Value);
        }
        
        return BadRequest(result.Message);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAppointment(int id, UpdateAppointmentCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        
        if (result.Success)
        {
            return Ok(result.Value);
        }
        
        return BadRequest(result.Message);
    }
}
