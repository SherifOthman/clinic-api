using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Locations.Queries.GetStates;

public record GetStatesQuery(int CountryGeonameId) : IRequest<Result<List<GeoNamesLocationDto>>>;

public class GetStatesQueryHandler : IRequestHandler<GetStatesQuery, Result<List<GeoNamesLocationDto>>>
{
    private readonly IGeoNamesService _geoNamesService;

    public GetStatesQueryHandler(IGeoNamesService geoNamesService)
    {
        _geoNamesService = geoNamesService;
    }

    public async Task<Result<List<GeoNamesLocationDto>>> Handle(GetStatesQuery request, CancellationToken cancellationToken)
    {
        var states = await _geoNamesService.GetStatesAsync(request.CountryGeonameId, cancellationToken);
        return Result<List<GeoNamesLocationDto>>.Ok(states);
    }
}