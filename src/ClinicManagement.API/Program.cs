using ClinicManagement.API;
using ClinicManagement.Application;
using ClinicManagement.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddApi(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication(builder.Configuration);
    builder.Services.AddSerilog((services, lc) => lc
       .ReadFrom.Configuration(builder.Configuration)
       .ReadFrom.Services(services));

    var app = builder.Build();

    // Run migrations (seed data is now handled via HasData in configurations)
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ClinicManagement.Infrastructure.Data.ApplicationDbContext>();
        await context.Database.MigrateAsync();
    }

    app.UseAppConfigurations();
    
    // Log the URLs the application is listening on
    var urls = app.Urls.Any() ? string.Join(", ", app.Urls) : "http://localhost:5000, https://localhost:7000";
    Log.Information("Application is listening on: {Urls}", urls);
    
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