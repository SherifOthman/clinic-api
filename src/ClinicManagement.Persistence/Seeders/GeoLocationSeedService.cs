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

        // ── Step 1: Countries ─────────────────────────────────────────────────

        // Load IDs already in the database so we can skip them
        var existingCountryIds = await _db.GeoCountries
            .Select(c => c.GeonameId)
            .ToHashSetAsync(ct);

        var allCountries = await _geoNames.GetCountriesAsync(ct);

        // Only keep countries not already in the DB
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

        // Only insert states whose parent country is already in our DB
        var validCountryIds = await _db.GeoCountries
            .Select(c => c.GeonameId)
            .ToHashSetAsync(ct);

        var allStates = await _geoNames.GetStatesAsync(ct);

        var newStates = allStates
            .Where(s => !existingStateIds.Contains(s.GeonameId)       // not already in DB
                     && validCountryIds.Contains(s.CountryGeonameId))  // parent country exists
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

        var existingCityIds = await _db.GeoCities
            .Select(c => c.GeonameId)
            .ToHashSetAsync(ct);

        // Only insert cities whose parent state is already in our DB
        var validStateIds = await _db.GeoStates
            .Select(s => s.GeonameId)
            .ToHashSetAsync(ct);

        var allCities = await _geoNames.GetCitiesAsync(ct);

        var newCities = allCities
            .Where(c => !existingCityIds.Contains(c.GeonameId)     // not already in DB
                     && validStateIds.Contains(c.StateGeonameId))   // parent state exists
            .Select(c => new GeoCity
            {
                GeonameId      = c.GeonameId,
                StateGeonameId = c.StateGeonameId,
                // Truncate to 150 chars as a safety net (column max length)
                NameEn = c.NameEn.Length > 150 ? c.NameEn[..150] : c.NameEn,
                NameAr = c.NameAr.Length > 150 ? c.NameAr[..150] : c.NameAr,
            })
            .ToList();

        // Insert in batches of 10,000
        const int batchSize = 10_000;
        for (var i = 0; i < newCities.Count; i += batchSize)
        {
            var batch = newCities.Skip(i).Take(batchSize).ToList();
            await _db.GeoCities.AddRangeAsync(batch, ct);
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Cities: inserted {To:N0} of {Total:N0}",
                Math.Min(i + batchSize, newCities.Count), newCities.Count);
        }

        _logger.LogInformation("Cities: +{Added} added, {Skipped} already existed",
            newCities.Count, existingCityIds.Count);

        _logger.LogInformation("GeoLocation seed complete — Countries: {C}, States: {S}, Cities: {Ci}",
            newCountries.Count, newStates.Count, newCities.Count);
    }
}
