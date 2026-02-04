using ClinicManagement.API;
using ClinicManagement.Application;
using ClinicManagement.Infrastructure;
using ClinicManagement.Infrastructure.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting Clinic Management API");

    builder.Host.UseSerilog();

    builder.Services.AddApi(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication(builder.Configuration);

    var app = builder.Build();

    Log.Information("Application built successfully");

    // Temporarily disable database initialization to test application startup
    // using (var scope = app.Services.CreateScope())
    // {
    //     try
    //     {
    //         Log.Information("Creating service scope for database initialization...");
    //         var databaseInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializationService>();
    //         Log.Information("Database initializer service resolved, calling InitializeAsync...");
    //         await databaseInitializer.InitializeAsync();
    //         Log.Information("Database initialization completed successfully");
    //     }
    //     catch (Exception ex)
    //     {
    //         Log.Fatal(ex, "Database initialization failed: {Message}", ex.Message);
    //         throw;
    //     }
    // }

    Log.Information("Configuring application middleware...");
    app.UseAppConfigurations();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
