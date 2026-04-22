using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders;

/// <summary>
/// Seeds countries, states, and cities from GeoNames files.
/// Countries and states seed at startup (fast).
/// Cities seed via Hangfire CitySeedJob in the background.
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

    // Called at startup — seeds countries and states only (fast)
    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedCountriesAsync(ct);
        await SeedStatesAsync(ct);
    }

    // Called by Hangfire CitySeedJob — returns 0 when fully seeded
    public async Task<int> SeedCitiesJobAsync(CancellationToken ct = default)
    {
        var countBefore = await _db.GeoCities.CountAsync(ct);

        var expected = await _geoNames.GetExpectedCityCountAsync(ct);
        if (expected.HasValue && countBefore >= expected.Value)
        {
            _logger.LogInformation("Cities already seeded ({Count:N0})", countBefore);
            return 0;
        }

        await SeedCitiesAsync(ct);

        var inserted = await _db.GeoCities.CountAsync(ct) - countBefore;
        return (int)Math.Max(0, inserted);
    }

    // ── Countries ─────────────────────────────────────────────────────────────

    private async Task SeedCountriesAsync(CancellationToken ct)
    {
        var source   = await _geoNames.GetCountriesAsync(ct);
        var existing = await _db.GeoCountries.Select(c => c.GeonameId).ToHashSetAsync(ct);

        var toAdd = source
            .Where(c => !existing.Contains(c.GeonameId))
            .Select(c => new GeoCountry
            {
                GeonameId   = c.GeonameId,
                CountryCode = c.CountryCode,
                NameEn      = c.NameEn,
                NameAr      = c.NameAr,
            })
            .ToList();

        if (toAdd.Count > 0)
        {
            await _db.GeoCountries.AddRangeAsync(toAdd, ct);
            await _db.SaveChangesAsync(ct);
        }

        // Update Arabic names for existing rows
        var updated = 0;
        foreach (var c in source.Where(c => c.NameAr != c.NameEn))
            updated += await _db.Database.ExecuteSqlRawAsync(
                "UPDATE GeoCountries SET NameAr = {0} WHERE GeonameId = {1} AND NameAr != {0}",
                c.NameAr, c.GeonameId);

        _logger.LogInformation("Countries: +{Added} added, {Updated} Arabic names updated", toAdd.Count, updated);
    }

    // ── States ────────────────────────────────────────────────────────────────

    private async Task SeedStatesAsync(CancellationToken ct)
    {
        var source       = await _geoNames.GetStatesAsync(ct);
        var existing     = await _db.GeoStates.Select(s => s.GeonameId).ToHashSetAsync(ct);
        var validCountry = await _db.GeoCountries.Select(c => c.GeonameId).ToHashSetAsync(ct);

        var toAdd = source
            .Where(s => !existing.Contains(s.GeonameId) && validCountry.Contains(s.CountryGeonameId))
            .Select(s => new GeoState
            {
                GeonameId        = s.GeonameId,
                CountryGeonameId = s.CountryGeonameId,
                NameEn           = s.NameEn,
                NameAr           = s.NameAr,
            })
            .ToList();

        if (toAdd.Count > 0)
        {
            await _db.GeoStates.AddRangeAsync(toAdd, ct);
            await _db.SaveChangesAsync(ct);
        }

        var updated = 0;
        foreach (var s in source.Where(s => s.NameAr != s.NameEn))
            updated += await _db.Database.ExecuteSqlRawAsync(
                "UPDATE GeoStates SET NameAr = {0} WHERE GeonameId = {1} AND NameAr != {0}",
                s.NameAr, s.GeonameId);

        _logger.LogInformation("States: +{Added} added, {Updated} Arabic names updated", toAdd.Count, updated);
    }

    // ── Cities ────────────────────────────────────────────────────────────────

    private async Task SeedCitiesAsync(CancellationToken ct)
    {
        var validStates = await _db.GeoStates.Select(s => s.GeonameId).ToHashSetAsync(ct);
        if (validStates.Count == 0)
        {
            _logger.LogWarning("No states in DB — skipping city seeding.");
            return;
        }

        var allCities   = await _geoNames.GetCitiesAsync(ct);
        var existingIds = await _db.GeoCities.Select(c => c.GeonameId).ToHashSetAsync(ct);

        // Track (stateId, nameEn) case-insensitively to prevent duplicate key violations.
        // SQL Server's unique index is case-insensitive by default collation.
        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var toInsert = new List<GeoCity>();
        foreach (var c in allCities)
        {
            if (!validStates.Contains(c.StateGeonameId)) continue;
            if (existingIds.Contains(c.GeonameId)) continue;

            var nameKey = $"{c.StateGeonameId}|{c.NameEn}";
            if (!seenNames.Add(nameKey)) continue; // same name in same state — skip

            toInsert.Add(new GeoCity
            {
                GeonameId      = c.GeonameId,
                StateGeonameId = c.StateGeonameId,
                NameEn         = c.NameEn.Length > 150 ? c.NameEn[..150] : c.NameEn,
                NameAr         = c.NameAr.Length > 150 ? c.NameAr[..150] : c.NameAr,
            });
        }

        if (toInsert.Count == 0)
        {
            _logger.LogInformation("Cities: nothing new to insert.");
            return;
        }

        // Insert in batches to avoid giant transactions
        const int batchSize = 2_000;
        for (var i = 0; i < toInsert.Count; i += batchSize)
        {
            var batch = toInsert.Skip(i).Take(batchSize).ToList();
            await _db.GeoCities.AddRangeAsync(batch, ct);
            await _db.SaveChangesAsync(ct);
            _db.ChangeTracker.Clear();
            _logger.LogInformation("Cities: inserted {Done:N0}/{Total:N0}...", Math.Min(i + batchSize, toInsert.Count), toInsert.Count);
        }

        _logger.LogInformation("Cities: +{Count:N0} inserted.", toInsert.Count);
    }
}
