using ClinicManagement.API;
using ClinicManagement.Application;
using ClinicManagement.Infrastructure;
using ClinicManagement.Infrastructure.Persistence;
using ClinicManagement.Infrastructure.Persistence.Seeders;
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
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApi(builder.Configuration, builder.Environment);

    var app = builder.Build();

    if (app.Environment.EnvironmentName != "Testing")
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                await context.Database.MigrateAsync();
                Log.Information("Database migrated successfully with EF Core");

                var roleSeed = services.GetRequiredService<RoleSeedService>();
                await roleSeed.SeedRolesAsync();
                Log.Information("Roles seeded successfully");

                var superAdminSeed = services.GetRequiredService<SuperAdminSeedService>();
                await superAdminSeed.SeedSuperAdminAsync();
                Log.Information("SuperAdmin user seeded successfully");

                var clinicOwnerSeed = services.GetRequiredService<ClinicOwnerSeedService>();
                await clinicOwnerSeed.SeedClinicOwnerDataAsync();
                Log.Information("Clinic Owner demo data seeded successfully");
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

public partial class Program { }
