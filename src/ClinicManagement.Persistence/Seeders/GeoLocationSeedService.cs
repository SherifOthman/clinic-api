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

    public async Task SeedAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting GeoLocation seed...");

        // ── Cleanup: remove duplicate GeonameId rows left by previous bad runs ─
        // Previous versions used allCountries.zip (unfiltered) and could insert
        // the same GeonameId multiple times across restarts.
        var totalCityCount    = await _db.GeoCities.CountAsync(ct);
        var distinctCityCount = await _db.GeoCities.Select(c => c.GeonameId).Distinct().CountAsync(ct);
        if (totalCityCount != distinctCityCount)
        {
            _logger.LogWarning(
                "Duplicate cities detected: {Total:N0} rows but only {Distinct:N0} distinct GeonameIds. Truncating and re-seeding...",
                totalCityCount, distinctCityCount);
            await _db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE GeoCities", ct);
        }

        // ── Step 1: Countries ─────────────────────────────────────────────────

        var existingCountryIds = await _db.GeoCountries
            .Select(c => c.GeonameId)
            .ToHashSetAsync(ct);

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
            await _db.GeoCountries.AddRangeAsync(newCountries, ct);
            await _db.SaveChangesAsync(ct);
        }

        _logger.LogInformation("Countries: +{Added} added, {Skipped} already existed",
            newCountries.Count, existingCountryIds.Count);

        // ── Step 2: States ────────────────────────────────────────────────────

        var existingStateIds = await _db.GeoStates
            .Select(s => s.GeonameId)
            .ToHashSetAsync(ct);

        var validCountryIds = await _db.GeoCountries
            .Select(c => c.GeonameId)
            .ToHashSetAsync(ct);

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
            await _db.GeoStates.AddRangeAsync(newStates, ct);
            await _db.SaveChangesAsync(ct);
        }

        _logger.LogInformation("States: +{Added} added, {Skipped} already existed",
            newStates.Count, existingStateIds.Count);

        // ── Step 3: Cities ────────────────────────────────────────────────────

        var validStateIds = await _db.GeoStates
            .Select(s => s.GeonameId)
            .ToHashSetAsync(ct);

        // Load what's already in the DB — used to skip already-inserted cities
        var existingCityIds = await _db.GeoCities
            .Select(c => c.GeonameId)
            .ToHashSetAsync(ct);

        var allCities = await _geoNames.GetCitiesAsync(ct);

        // Build expected set from source (deduped by GeonameId, valid state only),
        // then exclude any GeonameId already in the DB.
        // Process in streaming batches to avoid loading 3.8M objects into memory at once.
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
            // Add inserted IDs so subsequent batches don't re-insert on restart
            foreach (var city in batch) existingCityIds.Add(city.GeonameId);
            totalInserted += batch.Count;
            _logger.LogInformation("Cities: inserted {Total:N0} so far...", totalInserted);
            batch.Clear();
        }

        // Flush remaining
        if (batch.Count > 0)
        {
            await _db.GeoCities.AddRangeAsync(batch, ct);
            await _db.SaveChangesAsync(ct);
            totalInserted += batch.Count;
        }

        if (totalInserted == 0)
            _logger.LogInformation("Cities: all {Count:N0} already seeded. Skipping.", existingCityIds.Count);
        else
            _logger.LogInformation("Cities: +{Added:N0} added", totalInserted);

        _logger.LogInformation("GeoLocation seed complete — Countries: {C}, States: {S}, Cities: {Ci}",
            newCountries.Count, newStates.Count, newCities.Count);
    }
}
