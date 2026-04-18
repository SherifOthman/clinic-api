using ClinicManagement.API;
using ClinicManagement.Application;
using ClinicManagement.Infrastructure;
using ClinicManagement.Persistence;
using ClinicManagement.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        ?? "Production"}.json", optional: true)
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting Clinic Management API");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    builder.Services.AddApplication();
    builder.Services.AddPersistence(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApi(builder.Configuration, builder.Environment);

    var app = builder.Build();

    if (app.Environment.EnvironmentName != "Testing")
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            try
            {
                await context.Database.MigrateAsync();
            }
            catch (Exception migEx)
            {
                Log.Warning(migEx, "Migration failed — this is expected on first deploy after a migration reset. Attempting EnsureCreated as fallback...");
                // Only drop+recreate in non-production environments
                if (app.Environment.IsProduction())
                {
                    Log.Error("Migration failed in Production. Manual intervention required. Skipping drop/recreate to protect data.");
                    throw;
                }
                await context.Database.EnsureDeletedAsync();
                await context.Database.MigrateAsync();
            }

            Log.Information("Database migrated successfully");

            await services.GetRequiredService<RoleSeedService>().SeedRolesAsync();
            await services.GetRequiredService<SpecializationSeedService>().SeedSpecializationsAsync();
            await services.GetRequiredService<ChronicDiseaseSeedService>().SeedChronicDiseasesAsync();
            await services.GetRequiredService<SubscriptionPlanSeedService>().SeedSubscriptionPlansAsync();
            await services.GetRequiredService<DemoUsersSeedService>().SeedAsync();
            await services.GetRequiredService<GeoLocationSeedService>().SeedCountriesAndStatesAsync();
            // Cities seeding runs in background — too large for foreground on shared hosting

            Log.Information("Database seeded successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while initializing the database");
        }
    }

    app.UseSerilogRequestLogging();
    app.UseAppConfigurations();

    Log.Information("Clinic Management API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
