using ClinicManagement.Persistence;
using ClinicManagement.Persistence.Seeders;
using ClinicManagement.Persistence.Seeders.Demo;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ClinicManagement.API;

public static class DatabaseInitialiser
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services    = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            try
            {
                await context.Database.MigrateAsync();
                Log.Information("Database migrated successfully");
            }
            catch (Exception migEx)
            {
                if (await context.Database.CanConnectAsync())
                    Log.Warning(migEx, "MigrateAsync failed but DB is reachable — schema assumed correct, continuing...");
                else
                {
                    Log.Error(migEx, "MigrateAsync failed and DB is unreachable");
                    throw;
                }
            }

            // Core seeds — fast, must complete before API serves requests
            await services.GetRequiredService<RoleSeedService>().SeedRolesAsync();
            await services.GetRequiredService<SpecializationSeedService>().SeedSpecializationsAsync();
            await services.GetRequiredService<ChronicDiseaseSeedService>().SeedChronicDiseasesAsync();
            await services.GetRequiredService<SubscriptionPlanSeedService>().SeedSubscriptionPlansAsync();
            await services.GetRequiredService<SystemUserSeedService>().SeedAsync();

            Log.Information("Core database seeding completed — API is ready");

            // Demo data — only in Development/Staging, never in Production
            var env = app.Environment;
            if (env.IsDevelopment() || env.IsStaging())
            {
                await services.GetRequiredService<DemoDataSeedService>().SeedAsync();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Database initialisation failed — app will start but data may be missing");
        }

        // Geo seeding (countries, states, cities) runs entirely in the background.
        // Countries + states: ~270 + ~3800 rows, fast but still I/O.
        // Cities: ~225K rows, requires ~200MB download on first run.
        // The API starts immediately and geo data becomes available within minutes.
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(3)); // let the app fully start first
            await using var bgScope = app.Services.CreateAsyncScope();
            try
            {
                Log.Information("Background geo seeding started...");
                await bgScope.ServiceProvider
                    .GetRequiredService<GeoLocationSeedService>()
                    .SeedAsync();
                Log.Information("Background geo seeding completed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Background geo seeding failed");
            }
        });
    }
}
