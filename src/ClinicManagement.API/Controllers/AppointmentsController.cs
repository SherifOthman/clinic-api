using ClinicManagement.API.Extensions;
using ClinicManagement.API.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;
using ClinicManagement.Application.Features.Appointments.Queries.GetAppointments;
using ClinicManagement.Domain.Common.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : BaseApiController
{
    private readonly IMediator _mediator;

    public AppointmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AppointmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAppointments(
        [FromQuery] DateTime? date,
        [FromQuery] Guid? doctorId,
        [FromQuery] Guid? patientId,
        [FromQuery] Guid? appointmentTypeId,
        [FromQuery] AppointmentStatus? status,
        CancellationToken cancellationToken)
    {
        var query = new GetAppointmentsQuery(date, doctorId, patientId, appointmentTypeId, status);
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAppointment(CreateAppointmentDto createDto, CancellationToken cancellationToken)
    {
        var command = new CreateAppointmentCommand(createDto);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result.ToApiError());
            
        return Created($"/api/appointments", result.Value);
    }

    // Note: GetAppointmentTypes endpoint removed as it was accessing DbContext directly
    // If needed, create GetAppointmentTypesQuery + Handler
}
