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

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
