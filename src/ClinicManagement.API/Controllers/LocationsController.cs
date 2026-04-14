using ClinicManagement.API.Contracts.Locations;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Features.Locations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers;

[Route("api/locations")]
[EnableRateLimiting(RateLimitPolicies.AnonStatic)]
public class LocationsController : BaseApiController
{
    [HttpGet("countries")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<CountryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCountries(CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetCountriesQuery(), ct);
        return result.IsSuccess
            ? Ok(result.Value!.Select(c => new CountryResponse(c.GeonameId, c.NameEn, c.NameAr, c.CountryCode)))
            : HandleResult(result, "Failed to retrieve countries");
    }

    [HttpGet("countries/{countryGeonameId:int}/states")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<StateResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStates(
        [FromRoute] int countryGeonameId, CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetStatesQuery(countryGeonameId), ct);
        return result.IsSuccess
            ? Ok(result.Value!.Select(s => new StateResponse(s.GeonameId, s.NameEn, s.NameAr)))
            : HandleResult(result, "Failed to retrieve states");
    }

    [HttpGet("states/{stateGeonameId:int}/cities")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<CityResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCities(
        [FromRoute] int stateGeonameId, CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetCitiesQuery(stateGeonameId), ct);
        return result.IsSuccess
            ? Ok(result.Value!.Select(c => new CityResponse(c.GeonameId, c.NameEn, c.NameAr)))
            : HandleResult(result, "Failed to retrieve cities");
    }
}
