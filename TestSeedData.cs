using ClinicManagement.Infrastructure.Data;
using ClinicManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Application.Common.Interfaces;

// Simple test program to verify seed data works
var services = new ServiceCollection();

// Add logging
services.AddLogging(builder => builder.AddConsole());

// Add DbContext with in-memory database for testing
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("TestDb"));

// Add Identity
services.AddIdentity<User, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add mock services
services.AddScoped<ICurrentUserService, MockCurrentUserService>();
services.AddScoped<IDateTimeProvider, MockDateTimeProvider>();

// Add seed service
services.AddScoped<ISimpleSeedService, SimpleSeedService>();

var serviceProvider = services.BuildServiceProvider();

try
{
    using var scope = serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var seedService = scope.ServiceProvider.GetRequiredService<ISimpleSeedService>();
    
    Console.WriteLine("Starting seed data test...");
    
    // Ensure database is created
    await context.Database.EnsureCreatedAsync();
    
    // Run seed service
    await seedService.SeedBasicDataAsync();
    
    // Verify data was seeded
    var specializationCount = await context.Specializations.CountAsync();
    var measurementCount = await context.MeasurementAttributes.CountAsync();
    var chronicDiseaseCount = await context.ChronicDiseases.CountAsync();
    var appointmentTypeCount = await context.AppointmentTypes.CountAsync();
    var subscriptionPlanCount = await context.SubscriptionPlans.CountAsync();
    
    Console.WriteLine($"Seed data test completed successfully!");
    Console.WriteLine($"- Specializations: {specializationCount}");
    Console.WriteLine($"- Measurement Attributes: {measurementCount}");
    Console.WriteLine($"- Chronic Diseases: {chronicDiseaseCount}");
    Console.WriteLine($"- Appointment Types: {appointmentTypeCount}");
    Console.WriteLine($"- Subscription Plans: {subscriptionPlanCount}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error during seed data test: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}

// Mock services for testing
public class MockCurrentUserService : ICurrentUserService
{
    public Guid? UserId => null;
    public Guid? ClinicId => null;
    public string? Email => null;
    public string IpAddress => "127.0.0.1";
    public string? UserAgent => null;
    public IEnumerable<string> Roles => new List<string>();
    public bool IsAuthenticated => false;

    public Guid GetRequiredUserId() => throw new InvalidOperationException("No user context available");
    public Guid GetRequiredClinicId() => throw new InvalidOperationException("No clinic context available");
    public bool TryGetUserId(out Guid userId) { userId = Guid.Empty; return false; }
    public bool TryGetClinicId(out Guid clinicId) { clinicId = Guid.Empty; return false; }
}

public class MockDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
}