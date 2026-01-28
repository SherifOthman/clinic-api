using ClinicManagement.Application.Features.Locations.Queries.GetCities;
using ClinicManagement.Application.Features.Locations.Queries.GetCountries;
using ClinicManagement.Application.Features.Locations.Queries.GetStates;
using ClinicManagement.Application.Features.Locations.Queries.SearchCities;
using ClinicManagement.Application.Features.Locations.Commands.ValidatePhone;
using ClinicManagement.Application.Features.Locations.Queries.GetGeoNamesHealth;
using ClinicManagement.Application.Features.Locations.Queries.GetCountryPhoneCodes;
using ClinicManagement.Application.Features.Locations.Queries.GetLocationHierarchy;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
public class LocationsController : BaseApiController
{
    public LocationsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet("countries")]
    public async Task<IActionResult> GetCountries()
    {
        var result = await Mediator.Send(new GetCountriesQuery());
        return HandleResult(result);
    }

    [HttpGet("countries/{countryId}/states")]
    public async Task<IActionResult> GetStates(int countryId)
    {
        var result = await Mediator.Send(new GetStatesQuery(countryId));
        return HandleResult(result);
    }

    [HttpGet("countries/{countryId}/cities")]
    public async Task<IActionResult> GetCities(int countryId, [FromQuery] int? stateId = null)
    {
        var result = await Mediator.Send(new GetCitiesQuery(countryId, stateId));
        return HandleResult(result);
    }

    [HttpGet("countries/{countryCode}/cities/search")]
    public async Task<IActionResult> SearchCities(string countryCode, [FromQuery] string query)
    {
        var result = await Mediator.Send(new SearchCitiesQuery(countryCode, query));
        return HandleResult(result);
    }

    [HttpPost("validate-phone")]
    public async Task<IActionResult> ValidatePhoneNumber([FromBody] ValidatePhoneRequest request)
    {
        var result = await Mediator.Send(new ValidatePhoneCommand(request.PhoneNumber));
        return HandleResult(result);
    }

    [HttpGet("geonames/health")]
    public async Task<IActionResult> GetGeoNamesHealth()
    {
        var result = await Mediator.Send(new GetGeoNamesHealthQuery());
        return HandleResult(result);
    }

    [HttpGet("country-phone-codes")]
    public async Task<IActionResult> GetCountryPhoneCodes()
    {
        var result = await Mediator.Send(new GetCountryPhoneCodesQuery());
        return HandleResult(result);
    }

    [HttpGet("hierarchy/{geoNameId}")]
    public async Task<IActionResult> GetLocationHierarchy(int geoNameId)
    {
        var result = await Mediator.Send(new GetLocationHierarchyQuery(geoNameId));
        return HandleResult(result);
    }
}

public class ValidatePhoneRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
}