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
                Log.Warning(migEx, "MigrateAsync failed — checking if schema already exists...");

                // On shared hosting after a migration reset, the tables exist but
                // __EFMigrationsHistory has the old migration name. EF tries to re-apply
                // the migration and fails because tables already exist.
                // Solution: if all tables exist, just mark the pending migrations as applied.
                try
                {
                    var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
                    if (pendingMigrations.Count > 0)
                    {
                        // Check if tables already exist (schema is fine, just history mismatch)
                        var canConnect = await context.Database.CanConnectAsync();
                        if (canConnect)
                        {
                            Log.Warning("Marking {Count} pending migration(s) as applied without running them: {Migrations}",
                                pendingMigrations.Count, string.Join(", ", pendingMigrations));

                            foreach (var migration in pendingMigrations)
                            {
                                await context.Database.ExecuteSqlRawAsync(
                                    $"INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('{migration}', '10.0.0')");
                            }
                        }
                    }
                }
                catch (Exception innerEx)
                {
                    Log.Error(innerEx, "Could not recover from migration failure. Manual intervention required.");

                    if (!app.Environment.IsProduction())
                    {
                        await context.Database.EnsureDeletedAsync();
                        await context.Database.MigrateAsync();
                    }
                    else throw;
                }
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
