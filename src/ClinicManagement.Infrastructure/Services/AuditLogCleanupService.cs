using ClinicManagement.Domain.Entities;
using ClinicManagement.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class AuditLogCleanupService
{
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
        var cutoff  = DateTimeOffset.UtcNow.AddMonths(-12);
        var deleted = await _context.Set<AuditLog>()
            .Where(a => a.Timestamp < cutoff)
            .ExecuteDeleteAsync();

        if (deleted > 0)
            _logger.LogInformation("Audit log cleanup: removed {Count} entries older than {Cutoff:yyyy-MM-dd}", deleted, cutoff);
        else
            _logger.LogInformation("Audit log cleanup: no old entries found");
    }
}
