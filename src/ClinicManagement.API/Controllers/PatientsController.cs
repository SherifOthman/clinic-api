using ClinicManagement.API.Authorization;
using ClinicManagement.API.Contracts.Locations;
using ClinicManagement.API.Contracts.Patients;
using ClinicManagement.API.Models;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Patients.Commands;
using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace ClinicManagement.API.Controllers;

[Route("api/patients")]
public class PatientsController : BaseApiController
{
    [HttpGet]
    [RequirePermission(Permission.ViewPatients)]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(PaginatedResult<PatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatients(
        [FromQuery] string? searchTerm,
        [FromQuery] string? gender,
        [FromQuery] int? stateGeonameId,
        [FromQuery] int? cityGeonameId,
        [FromQuery] int? countryGeonameId,
        [FromQuery] PaginationRequest pagination,
        [FromQuery] string? sortBy = null,
        [FromQuery] string sortDirection = "asc",
        CancellationToken cancellationToken = default)
    {
        var query = new GetPatientsQuery(searchTerm, pagination.PageNumber, pagination.PageSize, sortBy, sortDirection, gender, StateGeonameId: stateGeonameId, CityGeonameId: cityGeonameId, CountryGeonameId: countryGeonameId);
        var result = await Sender.Send(query, cancellationToken);
        return HandleResult(result, "Failed to retrieve patients");
    }

    [HttpGet("location-options")]
    [RequirePermission(Permission.ViewPatients)]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(List<LocationOption>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLocationOptions(
        [FromQuery] int? countryGeonameId,
        [FromQuery] int? stateGeonameId,
        CancellationToken cancellationToken = default)
    {
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var query = new GetPatientLocationOptionsQuery(countryGeonameId, stateGeonameId, isSuperAdmin);
        var result = await Sender.Send(query, cancellationToken);
        return HandleResult(result, "Failed to retrieve location options");
    }

    [HttpGet("all")]
    [Authorize(Policy = "SuperAdmin")]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(PaginatedResult<PatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPatients(
        [FromQuery] string? searchTerm,
        [FromQuery] string? gender,
        [FromQuery] string? clinicSearch,
        [FromQuery] int? stateGeonameId,
        [FromQuery] int? cityGeonameId,
        [FromQuery] int? countryGeonameId,
        [FromQuery] PaginationRequest pagination,
        [FromQuery] string? sortBy = null,
        [FromQuery] string sortDirection = "asc",
        CancellationToken cancellationToken = default)
    {
        var query = new GetPatientsQuery(searchTerm, pagination.PageNumber, pagination.PageSize, sortBy, sortDirection, gender, clinicSearch, stateGeonameId, cityGeonameId, countryGeonameId, IsSuperAdmin: true);
        var result = await Sender.Send(query, cancellationToken);
        return HandleResult(result, "Failed to retrieve patients");
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(Permission.ViewPatients)]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(PatientDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientDetail(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetPatientDetailQuery(id), cancellationToken);
        return HandleResult(result, "Failed to retrieve patient detail");
    }

    [HttpGet("all/{id:guid}")]
    [Authorize(Policy = "SuperAdmin")]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(PatientDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientDetailAdmin(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetPatientDetailQuery(id, IsSuperAdmin: true), cancellationToken);
        return HandleResult(result, "Failed to retrieve patient detail");
    }

    [HttpPost]
    [RequirePermission(Permission.CreatePatient)]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(typeof(PatientDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePatient(
        [FromBody] CreatePatientCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(request, cancellationToken);
        if (!result.IsSuccess) return HandleResult(result, "Failed to create patient");
        return CreatedAtAction(nameof(GetPatientDetail), new { id = result.Value }, null);
    }

    [HttpPut("{id}")]
    [RequirePermission(Permission.EditPatient)]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePatient(
        [FromRoute] Guid id,
        [FromBody] UpdatePatientRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdatePatientCommand(
            id, request.FirstName, request.LastName, request.DateOfBirth, request.Gender,
            request.CountryGeonameId, request.StateGeonameId, request.CityGeonameId,
            request.BloodType, request.PhoneNumbers, request.ChronicDiseaseIds);
        return HandleNoContent(await Sender.Send(command, cancellationToken), "Failed to update patient");
    }

    [HttpDelete("{id}")]
    [RequirePermission(Permission.DeletePatient)]
    [EnableRateLimiting(RateLimitPolicies.UserDeletes)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePatient([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new DeletePatientCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : HandleResult(result, "Failed to delete patient");
    }

    [HttpPost("{id}/restore")]
    [Authorize(Policy = "SuperAdmin")]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestorePatient([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new RestorePatientCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : HandleResult(result, "Failed to restore patient");
    }
}
