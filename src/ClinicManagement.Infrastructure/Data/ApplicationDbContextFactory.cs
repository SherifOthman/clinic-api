using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ClinicManagement.Infrastructure.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../ClinicManagement.API"))
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(
            configuration.GetConnectionString("DefaultConnection"),
            b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

        // Create mock services for design time
        var mockCurrentUserService = new MockCurrentUserService();
        var mockDateTimeProvider = new MockDateTimeProvider();

        return new ApplicationDbContext(optionsBuilder.Options, mockCurrentUserService, mockDateTimeProvider);
    }
}

// Mock services for design time
public class MockCurrentUserService : ClinicManagement.Application.Common.Interfaces.ICurrentUserService
{
    public Guid? UserId => null;
    public Guid? ClinicId => null;
    public string? Email => null;
    public string IpAddress => "127.0.0.1";
    public string? UserAgent => null;
    public IEnumerable<string> Roles => new List<string>();
    public bool IsAuthenticated => false;

    public Guid GetRequiredUserId() => throw new InvalidOperationException("No user context available at design time");
    public Guid GetRequiredClinicId() => throw new InvalidOperationException("No clinic context available at design time");
    public bool TryGetUserId(out Guid userId) { userId = Guid.Empty; return false; }
    public bool TryGetClinicId(out Guid clinicId) { clinicId = Guid.Empty; return false; }
}

public class MockDateTimeProvider : ClinicManagement.Application.Common.Interfaces.IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
}