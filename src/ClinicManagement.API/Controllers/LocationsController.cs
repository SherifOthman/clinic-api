using ClinicManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/locations")]
[Produces("application/json")]
public class LocationsController : ControllerBase
{
    private readonly GeoNamesService _geoNamesService;

    public LocationsController(GeoNamesService geoNamesService)
    {
        _geoNamesService = geoNamesService;
    }

    /// <summary>
    /// Get all countries
    /// </summary>
    [HttpGet("countries")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "LocationData")]
    [ProducesResponseType(typeof(List<CountryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCountries(CancellationToken ct)
    {
        var countries = await _geoNamesService.GetCountriesAsync();

        var response = countries
            .Select(c => new CountryDto(
                c.GeonameId,
                c.Name.En,
                c.Name.Ar,
                c.CountryCode))
            .OrderBy(c => c.CountryNameEn)
            .ToList();

        return Ok(response);
    }

    /// <summary>
    /// Get states by country geoname ID
    /// </summary>
    [HttpGet("countries/{countryGeonameId:int}/states")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "LocationData")]
    [ProducesResponseType(typeof(List<StateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStates([FromRoute] int countryGeonameId, CancellationToken ct)
    {
        var states = await _geoNamesService.GetStatesAsync(countryGeonameId, ct);

        var response = states
            .Select(s => new StateDto(
                s.GeonameId,
                s.Name.En,
                s.Name.Ar))
            .OrderBy(s => s.StateNameEn)
            .ToList();

        return Ok(response);
    }

    /// <summary>
    /// Get cities by state geoname ID
    /// </summary>
    [HttpGet("states/{stateGeonameId:int}/cities")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "LocationData")]
    [ProducesResponseType(typeof(List<CityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCities([FromRoute] int stateGeonameId, CancellationToken ct)
    {
        var cities = await _geoNamesService.GetCitiesAsync(stateGeonameId, ct);

        var response = cities
            .Select(c => new CityDto(
                c.GeonameId,
                c.Name.En,
                c.Name.Ar))
            .OrderBy(c => c.CityNameEn)
            .ToList();

        return Ok(response);
    }
}

public record CountryDto(
    int GeonameId,
    string CountryNameEn,
    string CountryNameAr,
    string CountryCode);

public record StateDto(
    int GeonameId,
    string StateNameEn,
    string StateNameAr);

public record CityDto(
    int GeonameId,
    string CityNameEn,
    string CityNameAr);
