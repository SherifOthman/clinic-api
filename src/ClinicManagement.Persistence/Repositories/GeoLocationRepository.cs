using ClinicManagement.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class GeoLocationRepository : IGeoLocationRepository
{
    private readonly ApplicationDbContext _db;

    public GeoLocationRepository(ApplicationDbContext db) => _db = db;

    public async Task<List<CountryItem>> GetCountriesAsync(CancellationToken ct = default)
        => await _db.GeoCountries
            .AsNoTracking()
            .OrderBy(c => c.NameEn)
            .Select(c => new CountryItem(c.GeonameId, c.NameEn, c.NameAr, c.CountryCode))
            .ToListAsync(ct);

    public async Task<List<StateItem>> GetStatesAsync(int countryGeonameId, CancellationToken ct = default)
        => await _db.GeoStates
            .AsNoTracking()
            .Where(s => s.CountryGeonameId == countryGeonameId)
            .OrderBy(s => s.NameEn)
            .Select(s => new StateItem(s.GeonameId, s.NameEn, s.NameAr))
            .ToListAsync(ct);

    public async Task<List<CityItem>> GetCitiesAsync(int stateGeonameId, CancellationToken ct = default)
        => await _db.GeoCities
            .AsNoTracking()
            .Where(c => c.StateGeonameId == stateGeonameId)
            .OrderBy(c => c.NameEn)
            .Select(c => new CityItem(c.GeonameId, c.NameEn, c.NameAr))
            .ToListAsync(ct);
}
