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

                var specializationSeed = services.GetRequiredService<SpecializationSeedService>();
                await specializationSeed.SeedSpecializationsAsync();
                Log.Information("Specializations seeded successfully");

                var chronicDiseaseSeed = services.GetRequiredService<ChronicDiseaseSeedService>();
                await chronicDiseaseSeed.SeedChronicDiseasesAsync();
                Log.Information("Chronic diseases seeded successfully");

                var subscriptionPlanSeed = services.GetRequiredService<SubscriptionPlanSeedService>();
                await subscriptionPlanSeed.SeedSubscriptionPlansAsync();
                Log.Information("Subscription plans seeded successfully");

                var superAdminSeed = services.GetRequiredService<SuperAdminSeedService>();
                await superAdminSeed.SeedAsync();
                Log.Information("SuperAdmin seeded");

                var clinicOwnerSeed = services.GetRequiredService<ClinicOwnerSeedService>();
                await clinicOwnerSeed.SeedAsync();
                Log.Information("ClinicOwner demo data seeded");

                var doctorSeed = services.GetRequiredService<DoctorSeedService>();
                await doctorSeed.SeedAsync();
                Log.Information("Doctor demo user seeded");

                var receptionistSeed = services.GetRequiredService<ReceptionistSeedService>();
                await receptionistSeed.SeedAsync();
                Log.Information("Receptionist demo user seeded");

                var patientSeed = services.GetRequiredService<PatientSeedService>();
                await patientSeed.SeedAsync();
                Log.Information("Demo patients seeded");
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
