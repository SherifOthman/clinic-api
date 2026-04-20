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

        // Countries + states are fast — always run to pick up Arabic name updates
        await _seeder.SeedCountriesAndStatesAsync();

        // Cities — returns 0 when fully seeded
        var inserted = await _seeder.SeedCitiesAsync();

        if (inserted == 0)
        {
            _logger.LogInformation("GeoSeedJob: all cities seeded — removing recurring job.");
            RecurringJob.RemoveIfExists("geo-seed");
        }
        else
        {
            _logger.LogInformation("GeoSeedJob: inserted {Count:N0} cities this pass.", inserted);
        }
    }
}
