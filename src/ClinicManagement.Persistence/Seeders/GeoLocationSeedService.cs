using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders;

/// <summary>
/// Inserts countries, states, and cities into the database using data from GeoNamesService.
///
/// SAFE TO RE-RUN: already-existing rows are skipped, only new ones are inserted.
/// This means you can restart the app and it won't duplicate data.
///
/// ORDER MATTERS:
///   1. Countries first  (states need a valid country ID)
///   2. States second    (cities need a valid state ID)
///   3. Cities last
/// </summary>
public class GeoLocationSeedService
{
    private readonly ApplicationDbContext _db;
    private readonly IGeoNamesService _geoNames;
    private readonly ILogger<GeoLocationSeedService> _logger;

    public GeoLocationSeedService(
        ApplicationDbContext db,
        IGeoNamesService geoNames,
        ILogger<GeoLocationSeedService> logger)
    {
        _db       = db;
        _geoNames = geoNames;
        _logger   = logger;
    }

    public async Task SeedCountriesAndStatesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Seeding countries and states...");

        var allCountries = await _geoNames.GetCountriesAsync(ct);

        var existingCountryIds = await _db.GeoCountries
            .Select(c => c.GeonameId)
            .ToHashSetAsync(ct);

        // Insert new countries
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
            await _db.GeoCountries.AddRangeAsync(newCountries, ct);
            await _db.SaveChangesAsync(ct);
        }

        // Update existing countries where NameAr = NameEn (seeded without Arabic names)
        var sourceMap = allCountries.ToDictionary(c => c.GeonameId);
        var toUpdate  = await _db.GeoCountries
            .Where(c => c.NameAr == c.NameEn)
            .ToListAsync(ct);

        var updatedCount = 0;
        foreach (var country in toUpdate)
        {
            if (!sourceMap.TryGetValue(country.GeonameId, out var src)) continue;
            if (src.NameAr == src.NameEn) continue; // Arabic still not available
            country.NameAr = src.NameAr;
            updatedCount++;
        }
        if (updatedCount > 0) await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Countries: +{Added} added, {Updated} Arabic names updated, {Skipped} already existed",
            newCountries.Count, updatedCount, existingCountryIds.Count - updatedCount);

        // ── States ────────────────────────────────────────────────────────────

        var allStates = await _geoNames.GetStatesAsync(ct);

        var existingStateIds = await _db.GeoStates
            .Select(s => s.GeonameId)
            .ToHashSetAsync(ct);

        var validCountryIds = await _db.GeoCountries
            .Select(c => c.GeonameId)
            .ToHashSetAsync(ct);

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
            await _db.GeoStates.AddRangeAsync(newStates, ct);
            await _db.SaveChangesAsync(ct);
        }

        // Update existing states where NameAr = NameEn
        var stateSourceMap = allStates.ToDictionary(s => s.GeonameId);
        var statesToUpdate  = await _db.GeoStates
            .Where(s => s.NameAr == s.NameEn)
            .ToListAsync(ct);

        var updatedStateCount = 0;
        foreach (var state in statesToUpdate)
        {
            if (!stateSourceMap.TryGetValue(state.GeonameId, out var src)) continue;
            if (src.NameAr == src.NameEn) continue;
            state.NameAr = src.NameAr;
            updatedStateCount++;
        }
        if (updatedStateCount > 0) await _db.SaveChangesAsync(ct);

        _logger.LogInformation("States: +{Added} added, {Updated} Arabic names updated, {Skipped} already existed",
            newStates.Count, updatedStateCount, existingStateIds.Count - updatedStateCount);
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedCountriesAndStatesAsync(ct);
        await SeedCitiesAsync(ct);
    }

    /// <summary>
    /// Seeds only cities. Safe to call repeatedly — skips already-inserted GeonameIds.
    /// Returns the number of cities inserted in this pass (0 = fully seeded).
    /// </summary>
    public async Task<int> SeedCitiesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting city seeding pass...");

        // ── Cleanup: remove duplicate GeonameId rows left by previous bad runs ─
        var totalCityCount    = await _db.GeoCities.CountAsync(ct);
        var distinctCityCount = await _db.GeoCities.Select(c => c.GeonameId).Distinct().CountAsync(ct);
        if (totalCityCount != distinctCityCount)
        {
            _logger.LogWarning(
                "Duplicate cities detected: {Total:N0} rows but only {Distinct:N0} distinct GeonameIds. Clearing...",
                totalCityCount, distinctCityCount);
            await _db.Database.ExecuteSqlRawAsync("DELETE FROM GeoCities", ct);
        }

        var validStateIds = await _db.GeoStates
            .Select(s => s.GeonameId)
            .ToHashSetAsync(ct);

        if (validStateIds.Count == 0)
        {
            _logger.LogWarning("No states found — skipping city seeding. Run countries+states seed first.");
            return 0;
        }

        var existingCityIds = await _db.GeoCities
            .Select(c => c.GeonameId)
            .ToHashSetAsync(ct);

        var allCities = await _geoNames.GetCitiesAsync(ct);

        const int batchSize = 10_000;
        var batch           = new List<GeoCity>(batchSize);
        var totalInserted   = 0;
        var seenInBatch     = new HashSet<int>();

        foreach (var c in allCities)
        {
            if (!validStateIds.Contains(c.StateGeonameId)) continue;
            if (existingCityIds.Contains(c.GeonameId)) continue;
            if (!seenInBatch.Add(c.GeonameId)) continue;

            batch.Add(new GeoCity
            {
                GeonameId      = c.GeonameId,
                StateGeonameId = c.StateGeonameId,
                NameEn         = c.NameEn.Length > 150 ? c.NameEn[..150] : c.NameEn,
                NameAr         = c.NameAr.Length > 150 ? c.NameAr[..150] : c.NameAr,
            });

            if (batch.Count < batchSize) continue;

            await _db.GeoCities.AddRangeAsync(batch, ct);
            await _db.SaveChangesAsync(ct);
            foreach (var city in batch) existingCityIds.Add(city.GeonameId);
            totalInserted += batch.Count;
            _logger.LogInformation("Cities: inserted {Total:N0} so far...", totalInserted);
            batch.Clear();
        }

        if (batch.Count > 0)
        {
            await _db.GeoCities.AddRangeAsync(batch, ct);
            await _db.SaveChangesAsync(ct);
            totalInserted += batch.Count;
        }

        if (totalInserted == 0)
            _logger.LogInformation("Cities: all {Count:N0} already seeded.", existingCityIds.Count);
        else
            _logger.LogInformation("Cities: +{Added:N0} inserted this pass.", totalInserted);

        return totalInserted;
    }
}
