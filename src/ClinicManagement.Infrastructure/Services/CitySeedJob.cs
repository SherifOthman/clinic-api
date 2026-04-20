using ClinicManagement.Persistence.Seeders;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Hangfire job that seeds cities in the background.
/// Runs every 2 minutes, inserts only missing cities, removes itself when done.
/// [DisableConcurrentExecution] prevents two runs overlapping.
/// </summary>
public class CitySeedJob
{
    private readonly GeoLocationSeedService _seeder;
    private readonly ILogger<CitySeedJob> _logger;

    public CitySeedJob(GeoLocationSeedService seeder, ILogger<CitySeedJob> logger)
    {
        _seeder = seeder;
        _logger = logger;
    }

    [DisableConcurrentExecution(timeoutInSeconds: 0)]
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("CitySeedJob: starting pass...");
        var inserted = await _seeder.SeedCitiesJobAsync();

        if (inserted == 0)
        {
            _logger.LogInformation("CitySeedJob: all cities seeded — removing job.");
            RecurringJob.RemoveIfExists("city-seed");
        }
        else
        {
            _logger.LogInformation("CitySeedJob: inserted {Count:N0} cities this pass.", inserted);
        }
    }
}
