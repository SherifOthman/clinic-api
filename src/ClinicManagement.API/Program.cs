using ClinicManagement.API;
using ClinicManagement.Application;
using ClinicManagement.Infrastructure;
using Serilog;
using DatabaseInitializationService = ClinicManagement.Infrastructure.Services.DatabaseInitializationService;

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
    
    // Clean Architecture layers
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    
    // API layer (endpoints, middleware, etc.)
    builder.Services.AddApi(builder.Configuration, builder.Environment);
    

    var app = builder.Build();

    // Skip database initialization in Testing environment (handled by test factory)
    if (app.Environment.EnvironmentName != "Testing")
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var dbInitializer = services.GetRequiredService<DatabaseInitializationService>();
                dbInitializer.InitializeAsync().GetAwaiter().GetResult();
                Log.Information("Database initialized and seeded successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while initializing the database");
            }
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

// Expose Program class to integration tests for WebApplicationFactory
public partial class Program { }
