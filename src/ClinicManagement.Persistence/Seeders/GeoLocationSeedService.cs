using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders;

/// <summary>
/// Seeds GeoCountries, GeoStates, GeoCities from GeoNames API — runs once at startup.
/// Fetches both EN and AR names in parallel per resource, then bulk-inserts.
/// Skips entirely if GeoCountries table already has data.
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

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await _context.GeoCountries.AnyAsync(ct))
        {
            _logger.LogInformation("GeoLocation tables already seeded — skipping");
            return;
        }

        _logger.LogInformation("Starting GeoLocation seed from GeoNames API...");

        // ── Countries ─────────────────────────────────────────────────────────
        var (countriesEn, countriesAr) = await FetchBothLangs(
            lang => _geoNames.GetCountriesAsync(lang, ct));

        var countries = countriesEn
            .Select(en =>
            {
                var ar = countriesAr.FirstOrDefault(c => c.GeonameId == en.GeonameId);
                return new GeoCountry
                {
                    GeonameId   = en.GeonameId,
                    CountryCode = en.CountryCode,
                    NameEn      = en.Name,
                    NameAr      = ar?.Name ?? en.Name,
                };
            })
            .ToList();

        await _context.GeoCountries.AddRangeAsync(countries, ct);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Count} countries", countries.Count);

        // ── States ────────────────────────────────────────────────────────────
        var allStates = new List<GeoState>();

        foreach (var country in countries)
        {
            var (statesEn, statesAr) = await FetchBothLangs(
                lang => _geoNames.GetStatesAsync(country.GeonameId, lang, ct));

            allStates.AddRange(statesEn.Select(en =>
            {
                var ar = statesAr.FirstOrDefault(s => s.GeonameId == en.GeonameId);
                return new GeoState
                {
                    GeonameId        = en.GeonameId,
                    CountryGeonameId = country.GeonameId,
                    NameEn           = en.Name,
                    NameAr           = ar?.Name ?? en.Name,
                };
            }));
        }

        await _context.GeoStates.AddRangeAsync(allStates, ct);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Count} states across {CountryCount} countries",
            allStates.Count, countries.Count);

        // ── Cities ────────────────────────────────────────────────────────────
        var allCities = new List<GeoCity>();

        foreach (var state in allStates)
        {
            var (citiesEn, citiesAr) = await FetchBothLangs(
                lang => _geoNames.GetCitiesAsync(state.GeonameId, lang, ct));

            allCities.AddRange(citiesEn.Select(en =>
            {
                var ar = citiesAr.FirstOrDefault(c => c.GeonameId == en.GeonameId);
                return new GeoCity
                {
                    GeonameId      = en.GeonameId,
                    StateGeonameId = state.GeonameId,
                    NameEn         = en.Name,
                    NameAr         = ar?.Name ?? en.Name,
                };
            }));

            // Batch save every 500 cities to avoid huge transactions
            if (allCities.Count >= 500)
            {
                await _context.GeoCities.AddRangeAsync(allCities, ct);
                await _context.SaveChangesAsync(ct);
                _logger.LogInformation("Saved batch of {Count} cities (running total)", allCities.Count);
                allCities.Clear();
            }
        }

        if (allCities.Count > 0)
        {
            await _context.GeoCities.AddRangeAsync(allCities, ct);
            await _context.SaveChangesAsync(ct);
        }

        _logger.LogInformation("GeoLocation seed complete");
    }

    /// <summary>Fetches EN and AR results in parallel.</summary>
    private static async Task<(List<T> En, List<T> Ar)> FetchBothLangs<T>(
        Func<string, Task<List<T>>> fetch)
    {
        var enTask = fetch("en");
        var arTask = fetch("ar");
        await Task.WhenAll(enTask, arTask);
        return (enTask.Result, arTask.Result);
    }
}
