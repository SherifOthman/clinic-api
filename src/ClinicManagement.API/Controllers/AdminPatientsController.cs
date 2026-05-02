using ClinicManagement.API.Models;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Admin.Patients;
using ClinicManagement.Application.Features.Patients.Commands;
using ClinicManagement.Application.Features.Patients.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers;

/// <summary>
/// Cross-tenant patient endpoints — SuperAdmin only.
/// All queries here bypass the tenant filter and operate across all clinics.
/// </summary>
[Route("api/admin/patients")]
[Authorize(Policy = "SuperAdmin")]
public class AdminPatientsController : BaseApiController
{
    [HttpGet]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(PaginatedResult<PatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatients(
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
        var query = new GetAdminPatientsQuery(
            searchTerm, pagination.PageNumber, pagination.PageSize,
            sortBy, sortDirection, gender, clinicSearch,
            StateGeonameId: stateGeonameId,
            CityGeonameId: cityGeonameId,
            CountryGeonameId: countryGeonameId);
        var result = await Sender.Send(query, cancellationToken);
        return HandleResult(result, "Failed to retrieve patients");
    }

    [HttpGet("location-options")]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(List<LocationOption>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLocationOptions(
        [FromQuery] int? countryGeonameId,
        [FromQuery] int? stateGeonameId,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(
            new GetAdminPatientLocationOptionsQuery(countryGeonameId, stateGeonameId),
            cancellationToken);
        return HandleResult(result, "Failed to retrieve location options");
    }

    [HttpGet("{id:guid}")]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(PatientDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientDetail(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetAdminPatientDetailQuery(id), cancellationToken);
        return HandleResult(result, "Failed to retrieve patient detail");
    }

    [HttpPatch("{id}/restore")]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestorePatient([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new RestorePatientCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : HandleResult(result, "Failed to restore patient");
    }
}
