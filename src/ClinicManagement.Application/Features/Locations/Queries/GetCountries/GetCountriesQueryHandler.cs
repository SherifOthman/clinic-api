using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Locations.Queries.GetCountries;

public record GetCountriesQuery : IRequest<Result<List<GeoNamesCountryDto>>>;

public class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, Result<List<GeoNamesCountryDto>>>
{
    private readonly IGeoNamesService _geoNamesService;

    public GetCountriesQueryHandler(IGeoNamesService geoNamesService)
    {
        _geoNamesService = geoNamesService;
    }

    public async Task<Result<List<GeoNamesCountryDto>>> Handle(GetCountriesQuery request, CancellationToken cancellationToken)
    {
        var countries = await _geoNamesService.GetCountriesAsync(cancellationToken);
        return Result<List<GeoNamesCountryDto>>.Ok(countries);
    }
}