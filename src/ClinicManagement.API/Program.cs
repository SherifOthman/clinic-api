using ClinicManagement.API;
using ClinicManagement.Application;
using ClinicManagement.Infrastructure;
using ClinicManagement.Infrastructure.Services;
using ClinicManagement.Persistence;
using ClinicManagement.Persistence.Jobs;
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

    // ── Hangfire recurring jobs ───────────────────────────────────────────────
    if (app.Services.GetService<Hangfire.IGlobalConfiguration>() is not null)
    {
        try
        {
            RecurringJob.AddOrUpdate<EmailQueueProcessorJob>           (nameof(EmailQueueProcessorJob),            j => j.ExecuteAsync(), "*/5 * * * *"); // every 5 min
            RecurringJob.AddOrUpdate<RefreshTokenCleanupService>       (nameof(RefreshTokenCleanupService),        j => j.ExecuteAsync(), "0 */6 * * *"); // every 6h
            RecurringJob.AddOrUpdate<AuditLogCleanupService>           (nameof(AuditLogCleanupService),            j => j.ExecuteAsync(), "0 0 * * *");   // daily midnight
            RecurringJob.AddOrUpdate<UsageMetricsAggregationJob>       (nameof(UsageMetricsAggregationJob),        j => j.ExecuteAsync(), "0 1 * * *");   // daily 1am
            RecurringJob.AddOrUpdate<SubscriptionExpiryNotificationJob>(nameof(SubscriptionExpiryNotificationJob), j => j.ExecuteAsync(), "0 9 * * *");   // daily 9am
            RecurringJob.RemoveIfExists("email-queue-processor");
            RecurringJob.RemoveIfExists("refresh-token-cleanup");
            RecurringJob.RemoveIfExists("audit-log-cleanup");
            RecurringJob.RemoveIfExists("usage-metrics-aggregation");
            RecurringJob.RemoveIfExists("subscription-expiry");
            RecurringJob.RemoveIfExists("city-seed");
            RecurringJob.RemoveIfExists("geo-seed");
        }
        catch (Exception hangfireEx)
        {
            Log.Warning(hangfireEx, "Hangfire recurring job registration failed — check the connection string");
        }
    }

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
