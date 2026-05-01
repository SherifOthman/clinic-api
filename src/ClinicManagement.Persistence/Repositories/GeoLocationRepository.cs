using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class GeoLocationRepository : IGeoLocationRepository
{
    private readonly DbSet<GeoCountry> _countries;
    private readonly DbSet<GeoState>   _states;
    private readonly DbSet<GeoCity>    _cities;

    public GeoLocationRepository(ApplicationDbContext db)
    {
        _countries = db.Set<GeoCountry>();
        _states    = db.Set<GeoState>();
        _cities    = db.Set<GeoCity>();
    }

    public async Task<List<CountryItem>> GetCountriesAsync(CancellationToken ct = default)
        => await _countries
            .AsNoTracking()
            .OrderBy(c => c.NameEn)
            .Select(c => new CountryItem(c.GeonameId, c.NameEn, c.NameAr, c.CountryCode))
            .ToListAsync(ct);

    public async Task<List<StateItem>> GetStatesAsync(int countryGeonameId, CancellationToken ct = default)
        => await _states
            .AsNoTracking()
            .Where(s => s.CountryGeonameId == countryGeonameId)
            .OrderBy(s => s.NameEn)
            .Select(s => new StateItem(s.GeonameId, s.NameEn, s.NameAr))
            .ToListAsync(ct);

    public async Task<List<CityItem>> GetCitiesAsync(int stateGeonameId, CancellationToken ct = default)
        => await _cities
            .AsNoTracking()
            .Where(c => c.StateGeonameId == stateGeonameId)
            .OrderBy(c => c.NameEn)
            .Select(c => new CityItem(c.GeonameId, c.NameEn, c.NameAr))
            .ToListAsync(ct);
}
