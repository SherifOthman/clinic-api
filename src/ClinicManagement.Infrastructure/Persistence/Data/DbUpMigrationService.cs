using DbUp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace ClinicManagement.Infrastructure.Persistence.Data;

public class DbUpMigrationService
{
    private readonly string _connectionString;
    private readonly ILogger<DbUpMigrationService> _logger;

    public DbUpMigrationService(IConfiguration configuration, ILogger<DbUpMigrationService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _logger = logger;
    }

    public void MigrateDatabase()
    {
        _logger.LogInformation("Starting database migration with DbUp...");

        EnsureDatabase.For.SqlDatabase(_connectionString);

        var upgrader = DeployChanges.To
            .SqlDatabase(_connectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .WithTransactionPerScript()
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            _logger.LogError(result.Error, "Database migration failed");
            throw result.Error;
        }

        _logger.LogInformation("Database migration completed successfully");
    }
}
