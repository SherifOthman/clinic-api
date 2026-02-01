using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Patients.Commands.CreatePatient;
using ClinicManagement.Application.Features.Patients.Commands.DeletePatient;
using ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;
using ClinicManagement.Application.Features.Patients.Queries.GetPatient;
using ClinicManagement.Application.Features.Patients.Queries.GetPatients;
using ClinicManagement.Application.Features.Patients.Queries.GetPatientsWithPagination;
using ClinicManagement.Domain.Common.Constants;
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
    public PatientsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.PatientManagement)]
    public async Task<IActionResult> GetPatients()
    {
        var result = await Mediator.Send(new GetPatientsQuery());
        return HandleResult(result);
    }

    [HttpGet("paginated")]
    [Authorize(Roles = RoleNames.PatientManagement)]
    [ProducesResponseType(typeof(PagedResult<PatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientsWithPagination(
        [FromQuery] GetPatientsWithPaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = RoleNames.PatientManagement)]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatient(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPatientQuery { Id = id };
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.PatientManagement)]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreatePatient(CreatePatientCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return HandleCreateResult(result, nameof(GetPatient), new { id = result.Value?.Id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.PatientManagement)]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePatient(Guid id, UpdatePatientCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(MessageCodes.Controller.ID_MISMATCH);

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.PatientManagement)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePatient(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeletePatientCommand { Id = id };
        var result = await Mediator.Send(command, cancellationToken);
        return HandleDeleteResult(result);
    }
}