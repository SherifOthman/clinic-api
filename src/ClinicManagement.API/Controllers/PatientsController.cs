using ClinicManagement.API.Contracts.Patients;
using ClinicManagement.API.Models;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Patients.Commands;
using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/patients")]
public class PatientsController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "RequireClinic")]
    [ProducesResponseType(typeof(PaginatedResult<PatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatients(
        [FromQuery] string? searchTerm,
        [FromQuery] string? gender,
        [FromQuery] string? stateSearch,
        [FromQuery] string? citySearch,
        [FromQuery] string? countrySearch,
        [FromQuery] PaginationRequest pagination,
        [FromQuery] string? sortBy = null,
        [FromQuery] string sortDirection = "asc",
        CancellationToken cancellationToken = default)
    {
        var query = new GetPatientsQuery(searchTerm, pagination.PageNumber, pagination.PageSize, sortBy, sortDirection, gender, StateSearch: stateSearch, CitySearch: citySearch, CountrySearch: countrySearch);
        var result = await Sender.Send(query, cancellationToken);
        return HandleResult(result, "Failed to retrieve patients");
    }

    [HttpGet("states")]
    [Authorize(Policy = "RequireClinic")]
    [ProducesResponseType(typeof(List<PatientStateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientStates(CancellationToken cancellationToken = default)
    {
        var isSuperAdmin = User.IsInRole(UserRoles.SuperAdmin);
        var result = await Sender.Send(new GetPatientStatesQuery(isSuperAdmin), cancellationToken);
        return HandleResult(result, "Failed to retrieve patient states");
    }

    [HttpGet("cities")]
    [Authorize(Policy = "RequireClinic")]
    [ProducesResponseType(typeof(List<PatientStateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientCities(CancellationToken cancellationToken = default)
    {
        var isSuperAdmin = User.IsInRole(UserRoles.SuperAdmin);
        var result = await Sender.Send(new GetPatientCitiesQuery(isSuperAdmin), cancellationToken);
        return HandleResult(result, "Failed to retrieve patient cities");
    }

    [HttpGet("countries")]
    [Authorize(Policy = "RequireClinic")]
    [ProducesResponseType(typeof(List<PatientStateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientCountries(CancellationToken cancellationToken = default)
    {
        var isSuperAdmin = User.IsInRole(UserRoles.SuperAdmin);
        var result = await Sender.Send(new GetPatientCountriesQuery(isSuperAdmin), cancellationToken);
        return HandleResult(result, "Failed to retrieve patient countries");
    }

    [HttpGet("all")]
    [Authorize(Policy = "SuperAdmin")]
    [ProducesResponseType(typeof(PaginatedResult<PatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPatients(
        [FromQuery] string? searchTerm,
        [FromQuery] string? gender,
        [FromQuery] string? clinicSearch,
        [FromQuery] string? stateSearch,
        [FromQuery] string? citySearch,
        [FromQuery] string? countrySearch,
        [FromQuery] PaginationRequest pagination,
        [FromQuery] string? sortBy = null,
        [FromQuery] string sortDirection = "asc",
        CancellationToken cancellationToken = default)
    {
        var query = new GetPatientsQuery(searchTerm, pagination.PageNumber, pagination.PageSize, sortBy, sortDirection, gender, clinicSearch, stateSearch, citySearch, countrySearch, IsSuperAdmin: true);
        var result = await Sender.Send(query, cancellationToken);
        return HandleResult(result, "Failed to retrieve patients");
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "RequireClinic")]
    [ProducesResponseType(typeof(PatientDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientDetail(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetPatientDetailQuery(id), cancellationToken);
        return HandleResult(result, "Failed to retrieve patient detail");
    }

    /// <summary>SuperAdmin: get patient detail bypassing tenant filter.</summary>
    [HttpGet("all/{id:guid}")]
    [Authorize(Policy = "SuperAdmin")]
    [ProducesResponseType(typeof(PatientDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientDetailAdmin(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetPatientDetailQuery(id, IsSuperAdmin: true), cancellationToken);
        return HandleResult(result, "Failed to retrieve patient detail");
    }

    [HttpPost]
    [Authorize(Policy = "RequireClinic")]
    [ProducesResponseType(typeof(PatientDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePatient(
        [FromBody] CreatePatientRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreatePatientCommand(
            request.FullName,
            request.DateOfBirth,
            request.Gender,
            request.CityNameEn,
            request.CityNameAr,
            request.StateNameEn,
            request.StateNameAr,
            request.CountryNameEn,
            request.CountryNameAr,
            request.BloodType,
            request.PhoneNumbers,
            request.ChronicDiseaseIds);

        var result = await Sender.Send(command, cancellationToken);
        if (!result.IsSuccess) return HandleResult(result, "Failed to create patient");

        return CreatedAtAction(nameof(GetPatientDetail), new { id = result.Value }, null);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "RequireClinic")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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
            request.CityNameEn,
            request.CityNameAr,
            request.StateNameEn,
            request.StateNameAr,
            request.CountryNameEn,
            request.CountryNameAr,
            request.BloodType,
            request.PhoneNumbers,
            request.ChronicDiseaseIds);

        return HandleNoContent(await Sender.Send(command, cancellationToken), "Failed to update patient");
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireClinicOwner")]
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
