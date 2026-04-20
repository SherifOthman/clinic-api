using ClinicManagement.API;
using ClinicManagement.Application;
using ClinicManagement.Infrastructure;
using ClinicManagement.Infrastructure.Services;
using ClinicManagement.Persistence;
using ClinicManagement.Persistence.Seeders;
using Hangfire;
using Serilog;

// ── Bootstrap logger (before DI is built) ────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
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
    builder.Services.AddApi(builder.Configuration);

    var app = builder.Build();

    // ── Database migration + seeding ─────────────────────────────────────────
    if (app.Environment.EnvironmentName != "Testing")
        await app.InitialiseDatabaseAsync();

    // ── Middleware pipeline ───────────────────────────────────────────────────
    app.UseSerilogRequestLogging();
    app.UseAppConfigurations();

    RecurringJob.AddOrUpdate<EmailQueueProcessorJob>           ("email-queue-processor",     j => j.ExecuteAsync(), "*/5 * * * *");
    RecurringJob.AddOrUpdate<RefreshTokenCleanupService>       ("refresh-token-cleanup",     j => j.ExecuteAsync(), "0 */6 * * *");
    RecurringJob.AddOrUpdate<AuditLogCleanupService>           ("audit-log-cleanup",         j => j.ExecuteAsync(), "0 0 * * *");
    RecurringJob.AddOrUpdate<UsageMetricsAggregationJob>       ("usage-metrics-aggregation", j => j.ExecuteAsync(), "0 1 * * *");
    RecurringJob.AddOrUpdate<SubscriptionExpiryNotificationJob>("subscription-expiry",       j => j.ExecuteAsync(), "0 9 * * *");
    RecurringJob.RemoveIfExists("city-seed"); // old job name cleanup
    // Geo seeding: every 2 min, inserts missing rows, removes itself when done
    RecurringJob.AddOrUpdate<GeoSeedJob>("geo-seed", j => j.ExecuteAsync(), "*/2 * * * *");

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
