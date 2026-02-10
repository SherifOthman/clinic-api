using ClinicManagement.API.Extensions;
using ClinicManagement.API.Models;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Patients.Commands.CreatePatient;
using ClinicManagement.Application.Features.Patients.Commands.DeletePatient;
using ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;
using ClinicManagement.Application.Features.Patients.Queries.GetPatientById;
using ClinicManagement.Application.Features.Patients.Queries.GetPatients;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class PatientsController : BaseApiController
{
    private readonly IMediator _mediator;

    public PatientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("paginated")]
    [ProducesResponseType(typeof(PagedResult<PatientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPatientsPaginated(
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string sortDirection = "desc",
        [FromQuery] Gender? gender = null,
        [FromQuery] DateTime? dateOfBirthFrom = null,
        [FromQuery] DateTime? dateOfBirthTo = null,
        [FromQuery] DateTime? createdFrom = null,
        [FromQuery] DateTime? createdTo = null,
        [FromQuery] int? minAge = null,
        [FromQuery] int? maxAge = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPatientsQuery(
            searchTerm,
            pageNumber,
            pageSize,
            sortBy,
            sortDirection,
            gender,
            dateOfBirthFrom,
            dateOfBirthTo,
            createdFrom,
            createdTo,
            minAge,
            maxAge);

        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPatientByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (!result.Success)
            return NotFound(result.ToApiError());
            
        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePatient(CreatePatientDto dto, CancellationToken cancellationToken)
    {
        var command = new CreatePatientCommand(dto);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result.ToApiError());
            
        return CreatedAtAction(nameof(GetPatientById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePatient(Guid id, UpdatePatientDto dto, CancellationToken cancellationToken)
    {
        var command = new UpdatePatientCommand(id, dto);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePatient(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeletePatientCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleDeleteResult(result);
    }
}
