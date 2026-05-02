using ClinicManagement.Application.Abstractions.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClinicManagement.Persistence;

/// <summary>
/// Used by EF Core tools (dotnet ef migrations add) at design time.
/// Uses a hardcoded local connection string so the tools don't need
/// to boot the full application host.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            "Server=localhost;Database=ClinicDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options, new DesignTimeCurrentUserService());
    }

    /// <summary>
    /// No-op implementation used only during design-time (migrations).
    /// Returns null for all identity properties — no HTTP context exists at design time.
    /// </summary>
    private sealed class DesignTimeCurrentUserService : ICurrentUserService
    {
        public Guid?   UserId      => null;
        public Guid?   MemberId    => null;
        public Guid?   ClinicId    => null;
        public string? CountryCode => null;
        public string? FullName    => null;
        public string? Username    => null;
        public string? Email       => null;
        public string  IpAddress   => "design-time";
        public string? UserAgent   => null;
        public IEnumerable<string> Roles => [];
        public bool IsAuthenticated => false;
        public Guid GetRequiredUserId()   => throw new InvalidOperationException("No user context at design time.");
        public Guid GetRequiredClinicId() => throw new InvalidOperationException("No clinic context at design time.");
    }
}
