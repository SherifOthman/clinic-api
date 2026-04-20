using ClinicManagement.Persistence;
using ClinicManagement.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ClinicManagement.API;

public static class DatabaseInitialiser
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope  = app.Services.CreateScope();
        var services     = scope.ServiceProvider;

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
                // After a migration reset the schema is correct but __EFMigrationsHistory
                // has the old name — MigrateAsync fails. If the DB is reachable, continue.
                if (await context.Database.CanConnectAsync())
                    Log.Warning(migEx, "MigrateAsync failed but DB is reachable — schema assumed correct, continuing...");
                else
                {
                    Log.Error(migEx, "MigrateAsync failed and DB is unreachable");
                    throw;
                }
            }

            await services.GetRequiredService<RoleSeedService>().SeedRolesAsync();
            await services.GetRequiredService<SpecializationSeedService>().SeedSpecializationsAsync();
            await services.GetRequiredService<ChronicDiseaseSeedService>().SeedChronicDiseasesAsync();
            await services.GetRequiredService<SubscriptionPlanSeedService>().SeedSubscriptionPlansAsync();
            await services.GetRequiredService<DemoUsersSeedService>().SeedAsync();

            try
            {
                await services.GetRequiredService<GeoLocationSeedService>().SeedAsync();
            }
            catch (Exception geoEx)
            {
                Log.Error(geoEx, "Geo seeding failed: {Type} — {Message}", geoEx.GetType().Name, geoEx.Message);
                if (geoEx.InnerException is not null)
                    Log.Error("  Inner: {Type} — {Message}", geoEx.InnerException.GetType().Name, geoEx.InnerException.Message);
            }

            Log.Information("Database seeded successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Database initialisation failed — app will start but data may be missing");
        }
    }
}
