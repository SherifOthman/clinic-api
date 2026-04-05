using ClinicManagement.API.Models;
using ClinicManagement.Application.Features.Patients.Commands;
using ClinicManagement.Application.Features.Patients.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/patients")]
public class PatientsController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "RequireClinic")]
    [Authorize(Roles = "ClinicOwner,Doctor,Receptionist")]
    [ProducesResponseType(typeof(PaginatedPatientsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatients(
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string sortDirection = "asc",
        [FromQuery] bool? isMale = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPatientsQuery(searchTerm, pageNumber, pageSize, sortBy, sortDirection, isMale);
        var result = await Sender.Send(query, cancellationToken);
        return HandleResult(result, "Failed to retrieve patients");
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "RequireClinic")]
    [Authorize(Roles = "ClinicOwner,Doctor,Receptionist")]
    [ProducesResponseType(typeof(PatientDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientDetail(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetPatientDetailQuery(id), cancellationToken);
        return HandleResult(result, "Failed to retrieve patient detail");
    }

    [HttpPost]
    [Authorize(Policy = "RequireClinic")]
    [Authorize(Roles = "ClinicOwner,Doctor,Receptionist")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePatient(
        [FromBody] CreatePatientRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreatePatientCommand(
            request.FullName,
            request.DateOfBirth,
            request.Gender,
            request.CountryGeoNameId,
            request.StateGeoNameId,
            request.CityGeoNameId,
            request.BloodType,
            request.PhoneNumbers,
            request.ChronicDiseaseIds);

        var result = await Sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetPatients), new { }, result.Value)
            : HandleResult(result, "Failed to create patient");
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "RequireClinic")]
    [Authorize(Roles = "ClinicOwner,Doctor,Receptionist")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePatient(
        [FromRoute] Guid id,
        [FromBody] UpdatePatientRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdatePatientCommand(
            id,
            request.FullName,
            request.DateOfBirth,
            request.Gender,
            request.CountryGeoNameId,
            request.StateGeoNameId,
            request.CityGeoNameId,
            request.BloodType,
            request.ChronicDiseaseIds);

        return HandleResult(await Sender.Send(command, cancellationToken), "Failed to update patient");
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireClinic")]
    [Authorize(Roles = "ClinicOwner")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePatient([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new DeletePatientCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : HandleResult(result, "Failed to delete patient");
    }

    /// <summary>Restore a soft-deleted patient. SuperAdmin only.</summary>
    [HttpPost("{id}/restore")]
    [Authorize(Policy = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestorePatient([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new RestorePatientCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : HandleResult(result, "Failed to restore patient");
    }
}

public record CreatePatientRequest(
    string FullName,
    string DateOfBirth,
    string Gender,
    int? CountryGeoNameId,
    int? StateGeoNameId,
    int? CityGeoNameId,
    string? BloodType,
    List<PhoneNumberDto> PhoneNumbers,
    List<Guid> ChronicDiseaseIds);

public record UpdatePatientRequest(
    string FullName,
    string DateOfBirth,
    string Gender,
    int? CountryGeoNameId,
    int? StateGeoNameId,
    int? CityGeoNameId,
    string? BloodType,
    List<Guid>? ChronicDiseaseIds = null);
