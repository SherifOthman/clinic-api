using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Locations;

public record GetCountriesQuery : IRequest<Result<List<CountryItem>>>;
public record GetStatesQuery(int CountryGeonameId) : IRequest<Result<List<StateItem>>>;
public record GetCitiesQuery(int StateGeonameId) : IRequest<Result<List<CityItem>>>;

public class GetCountriesHandler : IRequestHandler<GetCountriesQuery, Result<List<CountryItem>>>
{
    private readonly IGeoLocationRepository _geoLocations;
    public GetCountriesHandler(IGeoLocationRepository geoLocations) => _geoLocations = geoLocations;

    public async Task<Result<List<CountryItem>>> Handle(GetCountriesQuery request, CancellationToken ct)
        => Result.Success(await _geoLocations.GetCountriesAsync(ct));
}

public class GetStatesHandler : IRequestHandler<GetStatesQuery, Result<List<StateItem>>>
{
    private readonly IGeoLocationRepository _geoLocations;
    public GetStatesHandler(IGeoLocationRepository geoLocations) => _geoLocations = geoLocations;

    public async Task<Result<List<StateItem>>> Handle(GetStatesQuery request, CancellationToken ct)
        => Result.Success(await _geoLocations.GetStatesAsync(request.CountryGeonameId, ct));
}

public class GetCitiesHandler : IRequestHandler<GetCitiesQuery, Result<List<CityItem>>>
{
    private readonly IGeoLocationRepository _geoLocations;
    public GetCitiesHandler(IGeoLocationRepository geoLocations) => _geoLocations = geoLocations;

    public async Task<Result<List<CityItem>>> Handle(GetCitiesQuery request, CancellationToken ct)
        => Result.Success(await _geoLocations.GetCitiesAsync(request.StateGeonameId, ct));
}
