using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Locations.Queries.GetCountries;
using ClinicManagement.Application.Features.Locations.Queries.GetStates;
using ClinicManagement.Application.Features.Locations.Queries.GetCities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

/// <summary>
/// GeoNames proxy controller
/// Frontend communicates ONLY with this controller, NEVER directly with GeoNames
/// </summary>
[ApiController]
[Route("api/locations")]
[AllowAnonymous]
public class LocationsController : BaseApiController
{
    private readonly IMediator _mediator;

    public LocationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all countries from GeoNames (cached for 24 hours)
    /// </summary>
    [HttpGet("countries")]
    public async Task<IActionResult> GetCountries(CancellationToken cancellationToken)
    {
        var query = new GetCountriesQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get states for a country from GeoNames (cached for 24 hours)
    /// </summary>
    [HttpGet("states")]
    public async Task<IActionResult> GetStates([FromQuery] int countryGeonameId, CancellationToken cancellationToken)
    {
        var query = new GetStatesQuery(countryGeonameId);
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get cities for a state from GeoNames (cached for 24 hours)
    /// </summary>
    [HttpGet("cities")]
    public async Task<IActionResult> GetCities([FromQuery] int stateGeonameId, CancellationToken cancellationToken)
    {
        var query = new GetCitiesQuery(stateGeonameId);
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
}
