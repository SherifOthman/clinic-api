using ClinicManagement.Domain.Entities;
using ClinicManagement.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Jobs;

/// <summary>
/// Hangfire job that archives old audit log entries.
/// Runs daily at midnight (configured in Program.cs).
///
/// Strategy:
/// - Deletes in batches of 5,000 to avoid long-running transactions and log bloat.
/// - Default retention: 12 months (configurable via RetentionMonths).
/// - Security events (Login, LoginFailed, AccountLocked) are kept for 24 months.
/// </summary>
public class AuditLogCleanupService
{
    private const int RetentionMonths         = 1;
    private const int SecurityRetentionMonths = 3;
    private const int BatchSize               = 5_000;

    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuditLogCleanupService> _logger;

    public AuditLogCleanupService(
        ApplicationDbContext context,
        ILogger<AuditLogCleanupService> logger)
    {
        _context = context;
        _logger  = logger;
    }

    public async Task ExecuteAsync()
    {
        var standardCutoff = DateTimeOffset.UtcNow.AddMonths(-RetentionMonths);
        var securityCutoff = DateTimeOffset.UtcNow.AddMonths(-SecurityRetentionMonths);

        var securityActions = new[] { "LoginFailed", "LoginBlocked", "AccountLocked", "LoginSuccess" };

        _logger.LogInformation(
            "Audit log cleanup starting — standard cutoff: {Standard:yyyy-MM-dd}, security cutoff: {Security:yyyy-MM-dd}",
            standardCutoff, securityCutoff);

        var totalDeleted = 0;

        // Delete standard entries in batches
        int deleted;
        do
        {
            deleted = await _context.Set<AuditLog>()
                .Where(a => a.Timestamp < standardCutoff
                         && !securityActions.Contains(a.EntityType))
                .OrderBy(a => a.Timestamp)
                .Take(BatchSize)
                .ExecuteDeleteAsync();

            totalDeleted += deleted;
        }
        while (deleted == BatchSize); // keep going until a batch comes back smaller than BatchSize

        // Delete security entries with longer retention
        int secDeleted;
        do
        {
            secDeleted = await _context.Set<AuditLog>()
                .Where(a => a.Timestamp < securityCutoff
                         && securityActions.Contains(a.EntityType))
                .OrderBy(a => a.Timestamp)
                .Take(BatchSize)
                .ExecuteDeleteAsync();

            totalDeleted += secDeleted;
        }
        while (secDeleted == BatchSize);

        if (totalDeleted > 0)
            _logger.LogInformation("Audit log cleanup complete — removed {Total} entries", totalDeleted);
        else
            _logger.LogInformation("Audit log cleanup — no entries to remove");
    }
}
