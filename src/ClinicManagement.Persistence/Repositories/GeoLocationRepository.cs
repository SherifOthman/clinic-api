using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class GeoLocationRepository : IGeoLocationRepository
{
    private readonly ApplicationDbContext _db;

    public GeoLocationRepository(ApplicationDbContext db) => _db = db;

    public async Task<List<CountryItem>> GetCountriesAsync(string lang, CancellationToken ct = default)
    {
        var isAr = lang == "ar";
        var rows = await _db.GeoCountries
            .AsNoTracking()
            .Select(c => new { c.GeonameId, c.NameEn, c.NameAr, c.CountryCode })
            .ToListAsync(ct);

        return rows
            .Select(c => new CountryItem(c.GeonameId, isAr ? c.NameAr : c.NameEn, c.CountryCode))
            .OrderBy(c => c.Name)
            .ToList();
    }

    public async Task<List<StateItem>> GetStatesAsync(int countryGeonameId, string lang, CancellationToken ct = default)
    {
        var isAr = lang == "ar";
        var rows = await _db.GeoStates
            .AsNoTracking()
            .Where(s => s.CountryGeonameId == countryGeonameId)
            .Select(s => new { s.GeonameId, s.NameEn, s.NameAr })
            .ToListAsync(ct);

        return rows
            .Select(s => new StateItem(s.GeonameId, isAr ? s.NameAr : s.NameEn))
            .OrderBy(s => s.Name)
            .ToList();
    }

    public async Task<List<CityItem>> GetCitiesAsync(int stateGeonameId, string lang, CancellationToken ct = default)
    {
        var isAr = lang == "ar";
        var rows = await _db.GeoCities
            .AsNoTracking()
            .Where(c => c.StateGeonameId == stateGeonameId)
            .Select(c => new { c.GeonameId, c.NameEn, c.NameAr })
            .ToListAsync(ct);

        return rows
            .Select(c => new CityItem(c.GeonameId, isAr ? c.NameAr : c.NameEn))
            .OrderBy(c => c.Name)
            .ToList();
    }
}
