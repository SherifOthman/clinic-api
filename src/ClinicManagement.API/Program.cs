using ClinicManagement.API;
using ClinicManagement.API.Hangfire;
using ClinicManagement.Application;
using ClinicManagement.Infrastructure;
using ClinicManagement.Persistence;
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
    // Only register if Hangfire was configured (connection string present in AddHangfire)
    var hangfireConfigured = !string.IsNullOrEmpty(
        app.Configuration.GetConnectionString("DefaultConnection"));
    if (hangfireConfigured)
    {
        try   { HangfireJobRegistration.RegisterAll(); }
        catch (Exception ex) { Log.Warning(ex, "Hangfire job registration failed — check the connection string"); }
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
