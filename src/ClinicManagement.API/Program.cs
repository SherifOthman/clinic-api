using ClinicManagement.API;
using ClinicManagement.Application;
using ClinicManagement.Infrastructure;
using ClinicManagement.Infrastructure.Data;
using ClinicManagement.Infrastructure.Services;
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
                var dbMigration = services.GetRequiredService<DbUpMigrationService>();
                dbMigration.MigrateDatabase();
                Log.Information("Database migrated successfully with DbUp");

                // Seed SuperAdmin user with hashed password
                var superAdminSeed = services.GetRequiredService<SuperAdminSeedService>();
                await superAdminSeed.SeedSuperAdminAsync();
                Log.Information("SuperAdmin user seeded successfully");
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
