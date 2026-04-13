using ClinicManagement.API.Contracts.Locations;
using ClinicManagement.API.Contracts.Patients;
using ClinicManagement.API.Models;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Patients.Commands;
using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Controllers;

[Route("api/patients")]
public class PatientsController : BaseApiController
{
    private readonly ApplicationDbContext _db;

    public PatientsController(ApplicationDbContext db) => _db = db;
    [HttpGet]
    [Authorize(Policy = "RequireClinic")]
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

    // NOTE: /states, /cities, /countries endpoints removed.
    // The frontend now uses the GeoNames API directly via /api/locations.
    // Filtering is done by GeoNames ID, not by stored name strings.

    /// <summary>
    /// Returns all distinct location IDs from patients in this clinic,
    /// with names resolved from the seeded GeoNames DB — one round trip, no external API.
    /// </summary>
    [HttpGet("location-filter")]
    [Authorize(Policy = "RequireClinic")]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(PatientLocationFilterResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLocationFilter(
        [FromQuery] string lang = "en",
        CancellationToken cancellationToken = default)
    {
        var isSuperAdmin = User.IsInRole(UserRoles.SuperAdmin);
        var isAr = lang == "ar";

        // DB queries must be sequential — EF Core DbContext is not thread-safe
        var countryResult = await Sender.Send(new GetDistinctPatientCountryIdsQuery(isSuperAdmin), cancellationToken);
        var stateResult   = await Sender.Send(new GetDistinctPatientStateIdsQuery(isSuperAdmin), cancellationToken);
        var cityResult    = await Sender.Send(new GetDistinctPatientCityIdsQuery(isSuperAdmin), cancellationToken);

        var countryIds = countryResult.IsSuccess ? countryResult.Value ?? [] : [];
        var stateIds   = stateResult.IsSuccess   ? stateResult.Value   ?? [] : [];
        var cityIds    = cityResult.IsSuccess     ? cityResult.Value    ?? [] : [];

        // Resolve names from seeded DB — simple IN queries, no GeoNames API
        var countries = countryIds.Count == 0 ? [] : await _db.GeoCountries
            .AsNoTracking()
            .Where(c => countryIds.Contains(c.GeonameId))
            .Select(c => new LocationNameDto(c.GeonameId, isAr ? c.NameAr : c.NameEn))
            .ToListAsync(cancellationToken);

        var states = stateIds.Count == 0 ? [] : await _db.GeoStates
            .AsNoTracking()
            .Where(s => stateIds.Contains(s.GeonameId))
            .Select(s => new LocationNameDto(s.GeonameId, isAr ? s.NameAr : s.NameEn))
            .ToListAsync(cancellationToken);

        var cities = cityIds.Count == 0 ? [] : await _db.GeoCities
            .AsNoTracking()
            .Where(c => cityIds.Contains(c.GeonameId))
            .Select(c => new LocationNameDto(c.GeonameId, isAr ? c.NameAr : c.NameEn))
            .ToListAsync(cancellationToken);

        return Ok(new PatientLocationFilterResponse(countries, states, cities));
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
    [Authorize(Policy = "RequireClinic")]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
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
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(PatientDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientDetailAdmin(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetPatientDetailQuery(id, IsSuperAdmin: true), cancellationToken);
        return HandleResult(result, "Failed to retrieve patient detail");
    }

    [HttpPost]
    [Authorize(Policy = "RequireClinic")]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
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
            request.CountryGeonameId,
            request.StateGeonameId,
            request.CityGeonameId,
            request.BloodType,
            request.PhoneNumbers,
            request.ChronicDiseaseIds);

        var result = await Sender.Send(command, cancellationToken);
        if (!result.IsSuccess) return HandleResult(result, "Failed to create patient");

        return CreatedAtAction(nameof(GetPatientDetail), new { id = result.Value }, null);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "RequireClinic")]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
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
            request.CountryGeonameId,
            request.StateGeonameId,
            request.CityGeonameId,
            request.BloodType,
            request.PhoneNumbers,
            request.ChronicDiseaseIds);

        return HandleNoContent(await Sender.Send(command, cancellationToken), "Failed to update patient");
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireClinicOwner")]
    [EnableRateLimiting(RateLimitPolicies.UserDeletes)]
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
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestorePatient([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new RestorePatientCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : HandleResult(result, "Failed to restore patient");
    }
}
