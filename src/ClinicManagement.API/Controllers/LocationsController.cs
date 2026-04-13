using ClinicManagement.API.Contracts.Locations;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/locations")]
[Produces("application/json")]
[EnableRateLimiting(RateLimitPolicies.AnonStatic)]
public class LocationsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public LocationsController(ApplicationDbContext db) => _db = db;

    [HttpGet("countries")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<CountryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCountries(
        [FromQuery] string lang = "en", CancellationToken ct = default)
    {
        var isAr = lang == "ar";
        var countries = await _db.GeoCountries
            .AsNoTracking()
            .OrderBy(c => isAr ? c.NameAr : c.NameEn)
            .Select(c => new CountryResponse(
                c.GeonameId,
                isAr ? c.NameAr : c.NameEn,
                c.CountryCode))
            .ToListAsync(ct);

        return Ok(countries);
    }

    [HttpGet("countries/{countryGeonameId:int}/states")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<StateResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStates(
        [FromRoute] int countryGeonameId,
        [FromQuery] string lang = "en", CancellationToken ct = default)
    {
        var isAr = lang == "ar";
        var states = await _db.GeoStates
            .AsNoTracking()
            .Where(s => s.CountryGeonameId == countryGeonameId)
            .OrderBy(s => isAr ? s.NameAr : s.NameEn)
            .Select(s => new StateResponse(
                s.GeonameId,
                isAr ? s.NameAr : s.NameEn))
            .ToListAsync(ct);

        return Ok(states);
    }

    [HttpGet("states/{stateGeonameId:int}/cities")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<CityResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCities(
        [FromRoute] int stateGeonameId,
        [FromQuery] string lang = "en", CancellationToken ct = default)
    {
        var isAr = lang == "ar";
        var cities = await _db.GeoCities
            .AsNoTracking()
            .Where(c => c.StateGeonameId == stateGeonameId)
            .OrderBy(c => isAr ? c.NameAr : c.NameEn)
            .Select(c => new CityResponse(
                c.GeonameId,
                isAr ? c.NameAr : c.NameEn))
            .ToListAsync(ct);

        return Ok(cities);
    }
}
