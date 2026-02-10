using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Locations.Queries.GetCities;

public record GetCitiesQuery(int StateGeonameId) : IRequest<Result<List<GeoNamesLocationDto>>>;

public class GetCitiesQueryHandler : IRequestHandler<GetCitiesQuery, Result<List<GeoNamesLocationDto>>>
{
    private readonly IGeoNamesService _geoNamesService;

    public GetCitiesQueryHandler(IGeoNamesService geoNamesService)
    {
        _geoNamesService = geoNamesService;
    }

    public async Task<Result<List<GeoNamesLocationDto>>> Handle(GetCitiesQuery request, CancellationToken cancellationToken)
    {
        var cities = await _geoNamesService.GetCitiesAsync(request.StateGeonameId, cancellationToken);
        return Result<List<GeoNamesLocationDto>>.Ok(cities);
    }
}