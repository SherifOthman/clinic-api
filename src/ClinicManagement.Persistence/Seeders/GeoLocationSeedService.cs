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
        var allStates    = await _geoNames.GetStatesAsync(ct);

        // ── Countries ─────────────────────────────────────────────────────────

        var existingCountryIds = await _db.GeoCountries
            .Select(c => c.GeonameId)
            .ToHashSetAsync(ct);

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

        // Update Arabic names for existing countries using direct SQL (reliable, no EF tracking issues)
        var countryUpdates = allCountries
            .Where(c => existingCountryIds.Contains(c.GeonameId) && c.NameAr != c.NameEn)
            .ToList();

        var countryUpdated = 0;
        foreach (var src in countryUpdates)
        {
            countryUpdated += await _db.Database.ExecuteSqlRawAsync(
                "UPDATE GeoCountries SET NameAr = {0} WHERE GeonameId = {1} AND NameAr = NameEn",
                src.NameAr, src.GeonameId);
        }

        _logger.LogInformation("Countries: +{Added} added, {Updated} Arabic names updated",
            newCountries.Count, countryUpdated);

        // ── States ────────────────────────────────────────────────────────────

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

        var stateUpdates = allStates
            .Where(s => existingStateIds.Contains(s.GeonameId) && s.NameAr != s.NameEn)
            .ToList();

        var stateUpdated = 0;
        foreach (var src in stateUpdates)
        {
            stateUpdated += await _db.Database.ExecuteSqlRawAsync(
                "UPDATE GeoStates SET NameAr = {0} WHERE GeonameId = {1} AND NameAr = NameEn",
                src.NameAr, src.GeonameId);
        }

        _logger.LogInformation("States: +{Added} added, {Updated} Arabic names updated",
            newStates.Count, stateUpdated);
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

        var allCities = _geoNames.StreamCitiesAsync(ct);

        const int batchSize = 5_000; // smaller batches = less memory pressure
        var batch           = new List<GeoCity>(batchSize);
        var totalInserted   = 0;
        var seenInBatch     = new HashSet<int>();

        await foreach (var c in allCities.WithCancellation(ct))
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
            _db.ChangeTracker.Clear(); // free tracked entities
            foreach (var city in batch) existingCityIds.Add(city.GeonameId);
            totalInserted += batch.Count;
            _logger.LogInformation("Cities: inserted {Total:N0} so far...", totalInserted);
            batch.Clear();
        }

        if (batch.Count > 0)
        {
            await _db.GeoCities.AddRangeAsync(batch, ct);
            await _db.SaveChangesAsync(ct);
            _db.ChangeTracker.Clear();
            totalInserted += batch.Count;
        }

        if (totalInserted == 0)
            _logger.LogInformation("Cities: all {Count:N0} already seeded.", existingCityIds.Count);
        else
            _logger.LogInformation("Cities: +{Added:N0} inserted this pass.", totalInserted);

        return totalInserted;
    }
}
