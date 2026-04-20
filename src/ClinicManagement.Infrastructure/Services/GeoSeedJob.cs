using ClinicManagement.Persistence.Seeders;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Hangfire job that seeds geo data (countries, states, cities) in the background.
/// Runs every 10 minutes. Each run inserts only missing rows.
/// Removes itself from the schedule once all cities are fully seeded.
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

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("GeoSeedJob: starting pass...");

        // Countries + states — always run to insert missing rows and update Arabic names
        await _seeder.SeedCountriesAndStatesAsync();

        // Cities — returns 0 when fully seeded
        var citiesInserted = await _seeder.SeedCitiesAsync();

        // Only remove the job when cities are done AND Arabic names are applied
        // (NameAr != NameEn for at least some countries means Arabic is working)
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
