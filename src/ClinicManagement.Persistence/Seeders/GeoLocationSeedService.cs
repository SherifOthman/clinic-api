using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders;

/// <summary>
/// Upserts GeoCountries, GeoStates, GeoCities from GeoNames API.
/// - New rows are inserted.
/// - Existing rows are updated (names may change if filters or GeoNames data changed).
/// - Rows no longer returned by GeoNames are left in place (safe — patients may reference them).
/// Triggered via POST /api/admin/geo-seed — not on startup.
/// </summary>
public class GeoLocationSeedService
{
    private readonly ApplicationDbContext _context;
    private readonly IGeoNamesService _geoNames;
    private readonly ILogger<GeoLocationSeedService> _logger;

    public GeoLocationSeedService(
        ApplicationDbContext context,
        IGeoNamesService geoNames,
        ILogger<GeoLocationSeedService> logger)
    {
        _context  = context;
        _geoNames = geoNames;
        _logger   = logger;
    }

    public async Task<GeoSeedResult> SeedAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting GeoLocation upsert from GeoNames API...");
        var result = new GeoSeedResult();

        // ── Countries ─────────────────────────────────────────────────────────
        var (countriesEn, countriesAr) = await FetchBothLangs(
            lang => _geoNames.GetCountriesAsync(lang, ct));

        var existingCountryIds = await _context.GeoCountries
            .Select(c => c.GeonameId).ToHashSetAsync(ct);

        var toAddCountries    = new List<GeoCountry>();
        var toUpdateCountries = new List<GeoCountry>();

        foreach (var en in countriesEn)
        {
            var ar   = countriesAr.FirstOrDefault(c => c.GeonameId == en.GeonameId);
            var nameAr = ar?.Name ?? en.Name;

            if (existingCountryIds.Contains(en.GeonameId))
            {
                toUpdateCountries.Add(new GeoCountry
                {
                    GeonameId   = en.GeonameId,
                    CountryCode = en.CountryCode,
                    NameEn      = en.Name,
                    NameAr      = nameAr,
                });
            }
            else
            {
                toAddCountries.Add(new GeoCountry
                {
                    GeonameId   = en.GeonameId,
                    CountryCode = en.CountryCode,
                    NameEn      = en.Name,
                    NameAr      = nameAr,
                });
            }
        }

        if (toAddCountries.Count > 0)
            await _context.GeoCountries.AddRangeAsync(toAddCountries, ct);

        foreach (var c in toUpdateCountries)
            _context.GeoCountries.Update(c);

        await _context.SaveChangesAsync(ct);
        result.CountriesAdded   = toAddCountries.Count;
        result.CountriesUpdated = toUpdateCountries.Count;
        _logger.LogInformation("Countries: +{Added} updated:{Updated}", result.CountriesAdded, result.CountriesUpdated);

        // ── States ────────────────────────────────────────────────────────────
        var allCountries = toAddCountries.Concat(toUpdateCountries).ToList();

        var existingStateIds = await _context.GeoStates
            .Select(s => s.GeonameId).ToHashSetAsync(ct);

        var toAddStates    = new List<GeoState>();
        var toUpdateStates = new List<GeoState>();

        foreach (var country in allCountries)
        {
            var (statesEn, statesAr) = await FetchBothLangs(
                lang => _geoNames.GetStatesAsync(country.GeonameId, lang, ct));

            foreach (var en in statesEn)
            {
                var ar = statesAr.FirstOrDefault(s => s.GeonameId == en.GeonameId);
                var state = new GeoState
                {
                    GeonameId        = en.GeonameId,
                    CountryGeonameId = country.GeonameId,
                    NameEn           = en.Name,
                    NameAr           = ar?.Name ?? en.Name,
                };

                if (existingStateIds.Contains(en.GeonameId))
                    toUpdateStates.Add(state);
                else
                    toAddStates.Add(state);
            }

            // Throttle: 2 requests per country (childrenJSON EN + AR)
            await Task.Delay(100, ct);
        }

        if (toAddStates.Count > 0)
            await _context.GeoStates.AddRangeAsync(toAddStates, ct);

        foreach (var s in toUpdateStates)
            _context.GeoStates.Update(s);

        await _context.SaveChangesAsync(ct);
        result.StatesAdded   = toAddStates.Count;
        result.StatesUpdated = toUpdateStates.Count;
        _logger.LogInformation("States: +{Added} updated:{Updated}", result.StatesAdded, result.StatesUpdated);

        // ── Cities ────────────────────────────────────────────────────────────
        var allStates = toAddStates.Concat(toUpdateStates).ToList();

        var existingCityIds = await _context.GeoCities
            .Select(c => c.GeonameId).ToHashSetAsync(ct);

        var toAddCities    = new List<GeoCity>();
        var toUpdateCities = new List<GeoCity>();

        foreach (var state in allStates)
        {
            var (citiesEn, citiesAr) = await FetchBothLangs(
                lang => _geoNames.GetCitiesAsync(state.GeonameId, lang, ct));

            foreach (var en in citiesEn)
            {
                var ar   = citiesAr.FirstOrDefault(c => c.GeonameId == en.GeonameId);
                var city = new GeoCity
                {
                    GeonameId      = en.GeonameId,
                    StateGeonameId = state.GeonameId,
                    NameEn         = en.Name,
                    NameAr         = ar?.Name ?? en.Name,
                };

                if (existingCityIds.Contains(en.GeonameId))
                    toUpdateCities.Add(city);
                else
                    toAddCities.Add(city);
            }

            // Throttle: ~3 requests per state (getJSON + searchJSON x2 langs).
            // Free GeoNames account = 1000 credits/hour → safe at ~300ms per state.
            await Task.Delay(300, ct);

            // Batch save every 500 to avoid huge transactions
            if (toAddCities.Count + toUpdateCities.Count >= 500)
            {
                await FlushCities(toAddCities, toUpdateCities, ct);
                result.CitiesAdded   += toAddCities.Count;
                result.CitiesUpdated += toUpdateCities.Count;
                toAddCities.Clear();
                toUpdateCities.Clear();
            }
        }

        if (toAddCities.Count > 0 || toUpdateCities.Count > 0)
        {
            result.CitiesAdded   += toAddCities.Count;
            result.CitiesUpdated += toUpdateCities.Count;
            await FlushCities(toAddCities, toUpdateCities, ct);
        }

        _logger.LogInformation("Cities: +{Added} updated:{Updated}", result.CitiesAdded, result.CitiesUpdated);
        _logger.LogInformation("GeoLocation upsert complete: {Result}", result);
        return result;
    }

    private async Task FlushCities(List<GeoCity> toAdd, List<GeoCity> toUpdate, CancellationToken ct)
    {
        if (toAdd.Count > 0)
            await _context.GeoCities.AddRangeAsync(toAdd, ct);
        foreach (var c in toUpdate)
            _context.GeoCities.Update(c);
        await _context.SaveChangesAsync(ct);
    }

    private static async Task<(List<T> En, List<T> Ar)> FetchBothLangs<T>(
        Func<string, Task<List<T>>> fetch)
    {
        var enTask = fetch("en");
        var arTask = fetch("ar");
        await Task.WhenAll(enTask, arTask);
        return (enTask.Result, arTask.Result);
    }
}

public record GeoSeedResult
{
    public int CountriesAdded   { get; set; }
    public int CountriesUpdated { get; set; }
    public int StatesAdded      { get; set; }
    public int StatesUpdated    { get; set; }
    public int CitiesAdded      { get; set; }
    public int CitiesUpdated    { get; set; }
}
