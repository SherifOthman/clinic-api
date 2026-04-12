using ClinicManagement.Domain.Entities;
using ClinicManagement.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class AuditLogCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditLogCleanupService> _logger;

    public AuditLogCleanupService(IServiceProvider serviceProvider, ILogger<AuditLogCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger          = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Audit Log Cleanup Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOldLogsAsync(stoppingToken);

                var now          = DateTimeOffset.UtcNow;
                var nextMidnight = now.UtcDateTime.Date.AddDays(1);
                var delay        = nextMidnight - now.UtcDateTime;
                _logger.LogInformation("Next audit log cleanup scheduled for {NextRun} UTC", nextMidnight);
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during audit log cleanup");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

    private async Task CleanupOldLogsAsync(CancellationToken cancellationToken)
    {
        using var scope   = _serviceProvider.CreateScope();
        var context       = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var auditLogs     = context.Set<AuditLog>();
        var cutoff        = DateTimeOffset.UtcNow.AddMonths(-12);

        var deleted = await auditLogs
            .Where(a => a.Timestamp < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted > 0)
            _logger.LogInformation("Audit log cleanup: removed {Count} entries older than {Cutoff:yyyy-MM-dd}", deleted, cutoff);
        else
            _logger.LogInformation("Audit log cleanup: no old entries found");
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Audit Log Cleanup Service stopping");
        await base.StopAsync(stoppingToken);
    }
}
