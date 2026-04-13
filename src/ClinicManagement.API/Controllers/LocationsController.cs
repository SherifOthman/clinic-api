using ClinicManagement.API.Contracts.Locations;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/locations")]
[Produces("application/json")]
[EnableRateLimiting(RateLimitPolicies.AnonStatic)]
public class LocationsController : ControllerBase
{
    private readonly GeoNamesService _geoNames;

    public LocationsController(GeoNamesService geoNames) => _geoNames = geoNames;

    [HttpGet("countries")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<CountryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCountries(
        [FromQuery] string lang = "en", CancellationToken ct = default)
    {
        var countries = await _geoNames.GetCountriesAsync(lang, ct);
        var response  = countries
            .Select(c => new CountryResponse(c.GeonameId, c.Name, c.CountryCode))
            .ToList();
        return Ok(response);
    }

    [HttpGet("countries/{countryGeonameId:int}/states")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<StateResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStates(
        [FromRoute] int countryGeonameId,
        [FromQuery] string lang = "en", CancellationToken ct = default)
    {
        var states   = await _geoNames.GetStatesAsync(countryGeonameId, lang, ct);
        var response = states
            .Select(s => new StateResponse(s.GeonameId, s.Name))
            .ToList();
        return Ok(response);
    }

    [HttpGet("states/{stateGeonameId:int}/cities")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<CityResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCities(
        [FromRoute] int stateGeonameId,
        [FromQuery] string lang = "en", CancellationToken ct = default)
    {
        var cities   = await _geoNames.GetCitiesAsync(stateGeonameId, lang, ct);
        var response = cities
            .Select(c => new CityResponse(c.GeonameId, c.Name))
            .ToList();
        return Ok(response);
    }
}
