using ClinicManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public interface IDatabaseInitializationService
{
    Task InitializeAsync();
}

public class DatabaseInitializationService : IDatabaseInitializationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseInitializationService> _logger;

    public DatabaseInitializationService(
        ApplicationDbContext context,
        ILogger<DatabaseInitializationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Starting database initialization...");
            
            // Check if database exists
            var canConnect = await _context.Database.CanConnectAsync();
            _logger.LogInformation("Database connection test: {CanConnect}", canConnect);
            
            if (!canConnect)
            {
                _logger.LogWarning("Cannot connect to database. Creating database...");
                await _context.Database.EnsureCreatedAsync();
            }
            
            // Apply pending migrations
            _logger.LogInformation("Checking for pending migrations...");
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            _logger.LogInformation("Found {Count} pending migrations", pendingMigrations.Count());
            
            if (pendingMigrations.Any())
            {
                _logger.LogInformation("Applying migrations...");
                await _context.Database.MigrateAsync();
                _logger.LogInformation("Migrations applied successfully");
            }
            else
            {
                _logger.LogInformation("No pending migrations found");
            }
            
            _logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization failed: {Message}", ex.Message);
            throw;
        }
    }
}
