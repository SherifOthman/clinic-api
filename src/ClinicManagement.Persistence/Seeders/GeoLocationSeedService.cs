using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders;

/// <summary>
/// Seeds countries, states, and cities from GeoNames files.
/// Countries and states seed at startup (fast, ~270 + ~3,800 rows).
/// Cities seed via Hangfire CitySeedJob in the background (~225K rows).
/// </summary>
public class GeoLocationSeedService
{
    private readonly ApplicationDbContext _db;
    private readonly IGeoNamesService _geoNames;
    private readonly ILogger<GeoLocationSeedService> _logger;

    public GeoLocationSeedService(ApplicationDbContext db, IGeoNamesService geoNames, ILogger<GeoLocationSeedService> logger)
    {
        _db       = db;
        _geoNames = geoNames;
        _logger   = logger;
    }

    // ── Called at startup ─────────────────────────────────────────────────────

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedCountriesAsync(ct);
        await SeedStatesAsync(ct);
    }

    // ── Called by Hangfire CitySeedJob every 2 min until done ─────────────────

    public async Task<int> SeedCitiesJobAsync(CancellationToken ct = default)
    {
        var countBefore = await _db.GeoCities.CountAsync(ct);
        var expected    = await _geoNames.GetExpectedCityCountAsync(ct);

        if (expected.HasValue && countBefore >= expected.Value)
        {
            _logger.LogInformation("Cities already seeded ({Count:N0})", countBefore);
            return 0;
        }

        await SeedCitiesAsync(ct);
        return (int)Math.Max(0, await _db.GeoCities.CountAsync(ct) - countBefore);
    }

    // ── Countries ─────────────────────────────────────────────────────────────

    private async Task SeedCountriesAsync(CancellationToken ct)
    {
        var source      = await _geoNames.GetCountriesAsync(ct);
        var existingIds = await _db.GeoCountries.Select(c => c.GeonameId).ToHashSetAsync(ct);

        var newRows = source
            .Where(c => !existingIds.Contains(c.GeonameId))
            .Select(c => new GeoCountry { GeonameId = c.GeonameId, CountryCode = c.CountryCode, NameEn = c.NameEn, NameAr = c.NameAr })
            .ToList();

        if (newRows.Count > 0)
        {
            await _db.GeoCountries.AddRangeAsync(newRows, ct);
            await _db.SaveChangesAsync(ct);
        }

        // Update Arabic names for existing rows where source has a translation
        var updated = 0;
        foreach (var c in source.Where(c => c.NameAr != c.NameEn))
            updated += await _db.Database.ExecuteSqlRawAsync(
                "UPDATE GeoCountries SET NameAr = {0} WHERE GeonameId = {1} AND NameAr != {0}",
                c.NameAr, c.GeonameId);

        _logger.LogInformation("Countries: +{Added} added, {Updated} Arabic names updated", newRows.Count, updated);
    }

    // ── States ────────────────────────────────────────────────────────────────

    private async Task SeedStatesAsync(CancellationToken ct)
    {
        var source        = await _geoNames.GetStatesAsync(ct);
        var existingIds   = await _db.GeoStates.Select(s => s.GeonameId).ToHashSetAsync(ct);
        var validCountries = await _db.GeoCountries.Select(c => c.GeonameId).ToHashSetAsync(ct);

        var newRows = source
            .Where(s => !existingIds.Contains(s.GeonameId) && validCountries.Contains(s.CountryGeonameId))
            .Select(s => new GeoState { GeonameId = s.GeonameId, CountryGeonameId = s.CountryGeonameId, NameEn = s.NameEn, NameAr = s.NameAr })
            .ToList();

        if (newRows.Count > 0)
        {
            await _db.GeoStates.AddRangeAsync(newRows, ct);
            await _db.SaveChangesAsync(ct);
        }

        var updated = 0;
        foreach (var s in source.Where(s => s.NameAr != s.NameEn))
            updated += await _db.Database.ExecuteSqlRawAsync(
                "UPDATE GeoStates SET NameAr = {0} WHERE GeonameId = {1} AND NameAr != {0}",
                s.NameAr, s.GeonameId);

        _logger.LogInformation("States: +{Added} added, {Updated} Arabic names updated", newRows.Count, updated);
    }

    // ── Cities ────────────────────────────────────────────────────────────────

    private async Task SeedCitiesAsync(CancellationToken ct)
    {
        var validStates = await _db.GeoStates.Select(s => s.GeonameId).ToHashSetAsync(ct);
        if (validStates.Count == 0) { _logger.LogWarning("No states in DB — skipping city seeding."); return; }

        var allCities   = await _geoNames.GetCitiesAsync(ct);
        var existingIds = await _db.GeoCities.Select(c => c.GeonameId).ToHashSetAsync(ct);

        // seenNames prevents duplicate (state, name) pairs before they hit the DB unique index.
        // OrdinalIgnoreCase matches SQL Server's default case-insensitive collation.
        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var toInsert  = new List<GeoCity>();

        foreach (var c in allCities)
        {
            if (!validStates.Contains(c.StateGeonameId)) continue;
            if (existingIds.Contains(c.GeonameId)) continue;
            if (!seenNames.Add($"{c.StateGeonameId}|{c.NameEn}")) continue;

            toInsert.Add(new GeoCity
            {
                GeonameId      = c.GeonameId,
                StateGeonameId = c.StateGeonameId,
                NameEn         = c.NameEn.Length > 150 ? c.NameEn[..150] : c.NameEn,
                NameAr         = c.NameAr.Length > 150 ? c.NameAr[..150] : c.NameAr,
            });
        }

        if (toInsert.Count == 0) { _logger.LogInformation("Cities: nothing new to insert."); return; }

        const int BatchSize = 2_000;
        for (var i = 0; i < toInsert.Count; i += BatchSize)
        {
            await _db.GeoCities.AddRangeAsync(toInsert.Skip(i).Take(BatchSize), ct);
            await _db.SaveChangesAsync(ct);
            _db.ChangeTracker.Clear();
            _logger.LogInformation("Cities: {Done:N0}/{Total:N0} inserted...", Math.Min(i + BatchSize, toInsert.Count), toInsert.Count);
        }

        _logger.LogInformation("Cities: +{Count:N0} total inserted.", toInsert.Count);
    }
}
