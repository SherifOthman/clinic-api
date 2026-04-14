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
        [FromQuery] string lang = "en",
        CancellationToken cancellationToken = default)
    {
        var query = new GetPatientsQuery(searchTerm, pagination.PageNumber, pagination.PageSize, sortBy, sortDirection, gender, StateGeonameId: stateGeonameId, CityGeonameId: cityGeonameId, CountryGeonameId: countryGeonameId, Lang: lang);
        var result = await Sender.Send(query, cancellationToken);
        return HandleResult(result, "Failed to retrieve patients");
    }

    // NOTE: /states, /cities, /countries endpoints removed.
    // The frontend now uses the GeoNames API directly via /api/locations.
    // Filtering is done by GeoNames ID, not by stored name strings.

    /// <summary>
    /// Returns distinct location options from actual patient data in this clinic.
    /// - No params            → countries that have at least one patient
    /// - ?countryGeonameId=X  → states in that country that have at least one patient
    /// - ?stateGeonameId=X    → cities in that state that have at least one patient
    /// Names are resolved from the seeded GeoNames DB.
    /// </summary>
    [HttpGet("location-options")]
    [Authorize(Policy = "RequireClinic")]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(List<LocationOption>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLocationOptions(
        [FromQuery] int? countryGeonameId,
        [FromQuery] int? stateGeonameId,
        [FromQuery] string lang = "en",
        CancellationToken ct = default)
    {
        var isSuperAdmin = User.IsInRole(UserRoles.SuperAdmin);
        var isAr = lang == "ar";

        var patientQuery = isSuperAdmin
            ? _db.Set<Patient>().IgnoreQueryFilters().AsNoTracking()
            : _db.Set<Patient>().AsNoTracking();

        List<LocationOption> options;

        if (stateGeonameId.HasValue)
        {
            // Cities: distinct cities in this state that have at least one patient
            options = await patientQuery
                .Where(p => p.StateGeonameId == stateGeonameId.Value && p.CityGeonameId != null)
                .Select(p => p.CityGeonameId!.Value)
                .Distinct()
                .Join(_db.GeoCities, id => id, c => c.GeonameId,
                    (id, c) => new LocationOption(id, isAr ? c.NameAr : c.NameEn))
                .OrderBy(o => o.Name)
                .ToListAsync(ct);
        }
        else if (countryGeonameId.HasValue)
        {
            // States: distinct states in this country that have at least one patient
            options = await patientQuery
                .Where(p => p.CountryGeonameId == countryGeonameId.Value && p.StateGeonameId != null)
                .Select(p => p.StateGeonameId!.Value)
                .Distinct()
                .Join(_db.GeoStates, id => id, s => s.GeonameId,
                    (id, s) => new LocationOption(id, isAr ? s.NameAr : s.NameEn))
                .OrderBy(o => o.Name)
                .ToListAsync(ct);
        }
        else
        {
            // Countries: distinct countries that have at least one patient
            options = await patientQuery
                .Where(p => p.CountryGeonameId != null)
                .Select(p => p.CountryGeonameId!.Value)
                .Distinct()
                .Join(_db.GeoCountries, id => id, c => c.GeonameId,
                    (id, c) => new LocationOption(id, isAr ? c.NameAr : c.NameEn))
                .OrderBy(o => o.Name)
                .ToListAsync(ct);
        }

        return Ok(options);
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
        [FromQuery] string lang = "en",
        CancellationToken cancellationToken = default)
    {
        var query = new GetPatientsQuery(searchTerm, pagination.PageNumber, pagination.PageSize, sortBy, sortDirection, gender, clinicSearch, stateGeonameId, cityGeonameId, countryGeonameId, IsSuperAdmin: true, Lang: lang);
        var result = await Sender.Send(query, cancellationToken);
        return HandleResult(result, "Failed to retrieve patients");
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "RequireClinic")]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(PatientDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientDetail(Guid id, [FromQuery] string lang = "en", CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetPatientDetailQuery(id, Lang: lang), cancellationToken);
        return HandleResult(result, "Failed to retrieve patient detail");
    }

    /// <summary>SuperAdmin: get patient detail bypassing tenant filter.</summary>
    [HttpGet("all/{id:guid}")]
    [Authorize(Policy = "SuperAdmin")]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(PatientDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientDetailAdmin(Guid id, [FromQuery] string lang = "en", CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetPatientDetailQuery(id, IsSuperAdmin: true, Lang: lang), cancellationToken);
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
