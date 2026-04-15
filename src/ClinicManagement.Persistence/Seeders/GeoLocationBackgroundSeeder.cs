using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders;

/// <summary>
/// Runs GeoLocationSeedService on a background thread after the app has started.
/// This keeps startup fast — the API is ready immediately while geo data seeds in the background.
/// Cities will appear in the database progressively as seeding completes.
/// </summary>
public class GeoLocationBackgroundSeeder : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GeoLocationBackgroundSeeder> _logger;

    public GeoLocationBackgroundSeeder(
        IServiceScopeFactory scopeFactory,
        ILogger<GeoLocationBackgroundSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for the app to fully start and DB migration to complete
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        _logger.LogInformation("GeoLocation background seeder starting...");

        try
        {
            using var scope   = _scopeFactory.CreateScope();
            var seeder        = scope.ServiceProvider.GetRequiredService<GeoLocationSeedService>();
            await seeder.SeedAsync(stoppingToken);
            _logger.LogInformation("GeoLocation background seeder completed.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GeoLocation background seeder was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GeoLocation background seeder failed.");
        }
    }
}
