using ClinicManagement.API.Extensions;
using ClinicManagement.API.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Doctors.Commands.CreateDoctor;
using ClinicManagement.Application.Features.Doctors.Queries.GetDoctorById;
using ClinicManagement.Application.Features.Doctors.Queries.GetDoctors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/doctors")]
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DoctorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<DoctorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDoctors([FromQuery] int? specializationId, CancellationToken cancellationToken)
    {
        var query = new GetDoctorsQuery { SpecializationId = specializationId };
        var result = await _mediator.Send(query, cancellationToken);

        return result.Success
            ? Ok(result.Value)
            : BadRequest(result.ToApiError());
    }

    [HttpGet("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDoctorById(int id, CancellationToken cancellationToken)
    {
        var query = new GetDoctorByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);

        return result.Success
            ? Ok(result.Value)
            : NotFound(result.ToApiError());
    }

    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDoctor(CreateDoctorCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return result.Success
            ? CreatedAtAction(nameof(GetDoctorById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result.ToApiError());
    }
}
