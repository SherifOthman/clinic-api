using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders;

/// <summary>
/// Seeds GeoCountries, GeoStates, GeoCities from GeoNames bulk dump files.
/// No API credits, no rate limits — downloads tab-separated text files directly.
///
/// Re-run safe: skips countries/states/cities already in DB.
/// Each re-run only inserts what's missing.
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
        _logger.LogInformation("Starting GeoLocation seed from GeoNames dump files...");
        var result = new GeoSeedResult();

        // ── Countries ─────────────────────────────────────────────────────────
        var existingCountryIds = await _context.GeoCountries
            .Select(c => c.GeonameId).ToHashSetAsync(ct);

        var allCountries = await _geoNames.GetCountriesAsync(ct);
        var newCountries = allCountries
            .Where(c => !existingCountryIds.Contains(c.GeonameId))
            .Select(c => new GeoCountry
            {
                GeonameId   = c.GeonameId,
                CountryCode = c.CountryCode,
                NameEn      = c.NameEn,
                NameAr      = c.NameAr,
            })
            .ToList();

        if (newCountries.Count > 0)
        {
            await _context.GeoCountries.AddRangeAsync(newCountries, ct);
            await _context.SaveChangesAsync(ct);
        }

        result.CountriesAdded = newCountries.Count;
        _logger.LogInformation("Countries: +{Added} (skipped {Skip} existing)",
            result.CountriesAdded, existingCountryIds.Count);

        // ── States ────────────────────────────────────────────────────────────
        var existingStateIds = await _context.GeoStates
            .Select(s => s.GeonameId).ToHashSetAsync(ct);

        // Only insert states whose parent country exists in our DB
        var validCountryIds = await _context.GeoCountries
            .Select(c => c.GeonameId).ToHashSetAsync(ct);

        var allStates = await _geoNames.GetStatesAsync(ct);
        var newStates = allStates
            .Where(s => !existingStateIds.Contains(s.GeonameId)
                     && validCountryIds.Contains(s.CountryGeonameId))
            .Select(s => new GeoState
            {
                GeonameId        = s.GeonameId,
                CountryGeonameId = s.CountryGeonameId,
                NameEn           = s.NameEn,
                NameAr           = s.NameAr,
            })
            .ToList();

        if (newStates.Count > 0)
        {
            await _context.GeoStates.AddRangeAsync(newStates, ct);
            await _context.SaveChangesAsync(ct);
        }

        result.StatesAdded = newStates.Count;
        _logger.LogInformation("States: +{Added} (skipped {Skip} existing)",
            result.StatesAdded, existingStateIds.Count);

        // ── Cities ────────────────────────────────────────────────────────────
        var existingCityIds = await _context.GeoCities
            .Select(c => c.GeonameId).ToHashSetAsync(ct);

        var validStateIds = await _context.GeoStates
            .Select(s => s.GeonameId).ToHashSetAsync(ct);

        var allCities = await _geoNames.GetCitiesAsync(ct);
        var newCities = allCities
            .Where(c => !existingCityIds.Contains(c.GeonameId)
                     && validStateIds.Contains(c.StateGeonameId))
            .Select(c => new GeoCity
            {
                GeonameId      = c.GeonameId,
                StateGeonameId = c.StateGeonameId,
                NameEn         = c.NameEn,
                NameAr         = c.NameAr,
            })
            .ToList();

        // Batch insert to avoid huge single transaction
        const int batchSize = 1000;
        for (var i = 0; i < newCities.Count; i += batchSize)
        {
            var batch = newCities.Skip(i).Take(batchSize).ToList();
            await _context.GeoCities.AddRangeAsync(batch, ct);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Cities: inserted batch {From}-{To} of {Total}",
                i + 1, Math.Min(i + batchSize, newCities.Count), newCities.Count);
        }

        result.CitiesAdded = newCities.Count;
        _logger.LogInformation("Cities: +{Added} (skipped {Skip} existing)",
            result.CitiesAdded, existingCityIds.Count);

        _logger.LogInformation("GeoLocation seed complete: {Result}", result);
        return result;
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
