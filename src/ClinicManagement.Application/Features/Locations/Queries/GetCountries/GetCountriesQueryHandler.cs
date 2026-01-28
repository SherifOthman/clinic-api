using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Locations.Queries.GetCountries;

public class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, Result<List<CountryDto>>>
{
    private readonly ILocationsService _locationsService;

    public GetCountriesQueryHandler(ILocationsService locationsService)
    {
        _locationsService = locationsService;
    }

    public async Task<Result<List<CountryDto>>> Handle(GetCountriesQuery request, CancellationToken cancellationToken)
    {
        var countries = await _locationsService.GetCountriesAsync();
        return Result<List<CountryDto>>.Ok(countries);
    }
}
