using ClinicManagement.Persistence.Seeders;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Hangfire job that seeds cities in the background.
/// Runs every 10 minutes, inserts only missing cities, and removes itself
/// from the recurring schedule once all cities are fully seeded.
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

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("CitySeedJob: starting city seeding pass...");

        var inserted = await _seeder.SeedCitiesAsync();

        if (inserted == 0)
        {
            _logger.LogInformation("CitySeedJob: all cities already seeded — removing recurring job.");
            RecurringJob.RemoveIfExists("city-seed");
        }
        else
        {
            _logger.LogInformation("CitySeedJob: inserted {Count:N0} cities this pass.", inserted);
        }
    }
}
