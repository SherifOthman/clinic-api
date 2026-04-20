using ClinicManagement.API;
using ClinicManagement.Application;
using ClinicManagement.Infrastructure;
using ClinicManagement.Infrastructure.Services;
using ClinicManagement.Persistence;
using ClinicManagement.Persistence.Seeders;
using Hangfire;
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
                Log.Information("Database migrated successfully");
            }
            catch (Exception migEx)
            {
                // After a migration reset, the server DB has the correct schema but
                // __EFMigrationsHistory has the old migration name. MigrateAsync fails.
                // If the DB is reachable, assume schema is correct and continue.
                var canConnect = await context.Database.CanConnectAsync();
                if (canConnect)
                {
                    Log.Warning(migEx, "MigrateAsync failed but DB is reachable — assuming schema is correct, continuing with seeding...");
                }
                else
                {
                    Log.Error(migEx, "MigrateAsync failed and DB is unreachable. Cannot proceed.");
                    throw;
                }
            }

            await services.GetRequiredService<RoleSeedService>().SeedRolesAsync();
            await services.GetRequiredService<SpecializationSeedService>().SeedSpecializationsAsync();
            await services.GetRequiredService<ChronicDiseaseSeedService>().SeedChronicDiseasesAsync();
            await services.GetRequiredService<SubscriptionPlanSeedService>().SeedSubscriptionPlansAsync();
            await services.GetRequiredService<DemoUsersSeedService>().SeedAsync();
            await services.GetRequiredService<GeoLocationSeedService>().SeedAsync();

            Log.Information("Database seeded successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while initializing the database");
        }
    }

    app.UseSerilogRequestLogging();
    app.UseAppConfigurations();

    // Register recurring Hangfire jobs
    RecurringJob.AddOrUpdate<EmailQueueProcessorJob>(
        "email-queue-processor",
        job => job.ExecuteAsync(),
        "*/5 * * * *"); // every 5 minutes

    RecurringJob.AddOrUpdate<RefreshTokenCleanupService>(
        "refresh-token-cleanup",
        job => job.ExecuteAsync(),
        "0 */6 * * *"); // every 6 hours

    RecurringJob.AddOrUpdate<AuditLogCleanupService>(
        "audit-log-cleanup",
        job => job.ExecuteAsync(),
        "0 0 * * *"); // daily at midnight

    RecurringJob.AddOrUpdate<UsageMetricsAggregationJob>(
        "usage-metrics-aggregation",
        job => job.ExecuteAsync(),
        "0 1 * * *"); // daily at 1am

    RecurringJob.AddOrUpdate<SubscriptionExpiryNotificationJob>(
        "subscription-expiry-notifications",
        job => job.ExecuteAsync(),
        "0 9 * * *"); // daily at 9am

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
