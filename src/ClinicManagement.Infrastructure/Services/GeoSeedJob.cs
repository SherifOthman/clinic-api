using ClinicManagement.Persistence.Seeders;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Hangfire job that seeds geo data (countries, states, cities) in the background.
/// Runs every 2 minutes. Each run inserts only missing rows.
/// Uses a distributed lock so concurrent runs never overlap.
/// Removes itself from the schedule once fully seeded with Arabic names.
/// </summary>
public class GeoSeedJob
{
    private readonly GeoLocationSeedService _seeder;
    private readonly ILogger<GeoSeedJob> _logger;

    public GeoSeedJob(GeoLocationSeedService seeder, ILogger<GeoSeedJob> logger)
    {
        _seeder = seeder;
        _logger = logger;
    }

    [DisableConcurrentExecution(timeoutInSeconds: 0)] // skip if already running
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("GeoSeedJob: starting pass...");

        await _seeder.SeedCountriesAndStatesAsync();

        var citiesInserted = await _seeder.SeedCitiesAsync();

        if (citiesInserted == 0)
        {
            var arabicApplied = await _seeder.HasArabicNamesAsync();
            if (arabicApplied)
            {
                _logger.LogInformation("GeoSeedJob: fully seeded with Arabic names — removing recurring job.");
                RecurringJob.RemoveIfExists("geo-seed");
            }
            else
            {
                _logger.LogWarning("GeoSeedJob: cities done but Arabic names not yet applied — will retry next pass.");
            }
        }
        else
        {
            _logger.LogInformation("GeoSeedJob: inserted {Count:N0} cities this pass.", citiesInserted);
        }
    }
}
