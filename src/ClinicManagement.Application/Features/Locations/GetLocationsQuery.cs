using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Locations;

// ── Queries ───────────────────────────────────────────────────────────────────

public record GetCountriesQuery : IRequest<Result<List<CountryItem>>>;
public record GetStatesQuery(int CountryGeonameId) : IRequest<Result<List<StateItem>>>;
public record GetCitiesQuery(int StateGeonameId) : IRequest<Result<List<CityItem>>>;

// ── Handlers ──────────────────────────────────────────────────────────────────

public class GetCountriesHandler : IRequestHandler<GetCountriesQuery, Result<List<CountryItem>>>
{
    private readonly IUnitOfWork _uow;
    public GetCountriesHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<CountryItem>>> Handle(GetCountriesQuery request, CancellationToken ct)
        => Result.Success(await _uow.GeoLocations.GetCountriesAsync(ct));
}

public class GetStatesHandler : IRequestHandler<GetStatesQuery, Result<List<StateItem>>>
{
    private readonly IUnitOfWork _uow;
    public GetStatesHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<StateItem>>> Handle(GetStatesQuery request, CancellationToken ct)
        => Result.Success(await _uow.GeoLocations.GetStatesAsync(request.CountryGeonameId, ct));
}

public class GetCitiesHandler : IRequestHandler<GetCitiesQuery, Result<List<CityItem>>>
{
    private readonly IUnitOfWork _uow;
    public GetCitiesHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<CityItem>>> Handle(GetCitiesQuery request, CancellationToken ct)
        => Result.Success(await _uow.GeoLocations.GetCitiesAsync(request.StateGeonameId, ct));
}
