using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Locations.Queries.GetCities;
using ClinicManagement.Application.Features.Locations.Queries.GetCountries;
using ClinicManagement.Application.Features.Locations.Queries.GetStates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
[AllowAnonymous]
public class LocationsController : BaseApiController
{
    private readonly IMediator _mediator;

    public LocationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all countries
    /// </summary>
    [HttpGet("countries")]
    [ProducesResponseType(typeof(List<GeoNamesCountryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCountries(CancellationToken cancellationToken)
    {
        var query = new GetCountriesQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get states by country
    /// </summary>
    [HttpGet("states")]
    [ProducesResponseType(typeof(List<GeoNamesLocationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStates([FromQuery] int countryGeonameId, CancellationToken cancellationToken)
    {
        var query = new GetStatesQuery(countryGeonameId);
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get cities by state
    /// </summary>
    [HttpGet("cities")]
    [ProducesResponseType(typeof(List<GeoNamesLocationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCities([FromQuery] int stateGeonameId, CancellationToken cancellationToken)
    {
        var query = new GetCitiesQuery(stateGeonameId);
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
}
