using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.API.Infrastructure.Services;

public class DatabaseInitializationService
{
    private readonly ApplicationDbContext _context;
    private readonly ComprehensiveSeedService _seedService;
    private readonly ILogger<DatabaseInitializationService> _logger;

    public DatabaseInitializationService(
        ApplicationDbContext context,
        ComprehensiveSeedService seedService,
        ILogger<DatabaseInitializationService> logger)
    {
        _context = context;
        _seedService = seedService;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Starting database initialization...");
            
            // Check if database exists
            _logger.LogInformation("Testing database connection...");
            var canConnect = await _context.Database.CanConnectAsync();
            _logger.LogInformation("Database connection test: {CanConnect}", canConnect);
            
            if (!canConnect)
            {
                _logger.LogWarning("Cannot connect to database. Creating database...");
                await _context.Database.EnsureCreatedAsync();
                _logger.LogInformation("Database created successfully");
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
            
            // Seed reference data
            _logger.LogInformation("Starting reference data seeding...");
            await _seedService.SeedReferenceDataAsync();
            _logger.LogInformation("Reference data seeding completed");
            
            _logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization failed: {Message}", ex.Message);
            throw;
        }
    }
}
