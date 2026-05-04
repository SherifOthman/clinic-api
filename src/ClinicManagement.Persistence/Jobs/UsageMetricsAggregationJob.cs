using ClinicManagement.Domain.Entities;
using ClinicManagement.Persistence;
using ClinicManagement.Persistence.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Jobs;

public class UsageMetricsAggregationJob
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UsageMetricsAggregationJob> _logger;

    public UsageMetricsAggregationJob(
        ApplicationDbContext context,
        ILogger<UsageMetricsAggregationJob> logger)
    {
        _context = context;
        _logger  = logger;
    }

    public async Task ExecuteAsync()
    {
        var yesterday     = DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddDays(-1).Date);
        var activeClinics = await TenantGuard.AsSystemQuery(_context.Set<Clinic>())
            .Where(c => c.IsActive && !c.IsDeleted)
            .ToListAsync();

        _logger.LogInformation("Aggregating metrics for {Count} clinics for {Date}", activeClinics.Count, yesterday);

        var processed = 0; var errors = 0;

        foreach (var clinic in activeClinics)
        {
            try
            {
                var activeStaffCount = await TenantGuard.AsSystemQuery(_context.Set<ClinicMember>())
                    .CountAsync(m => m.ClinicId == clinic.Id && m.IsActive);

                var existing = await TenantGuard.AsSystemQuery(_context.Set<ClinicUsageMetrics>())
                    .FirstOrDefaultAsync(m => m.ClinicId == clinic.Id && m.MetricDate == yesterday);

                if (existing is not null)
                {
                    existing.ActiveStaffCount  = activeStaffCount;
                    existing.LastAggregatedAt  = DateTimeOffset.UtcNow;
                }
                else
                {
                    _context.Set<ClinicUsageMetrics>().Add(new ClinicUsageMetrics
                    {
                        ClinicId           = clinic.Id,
                        MetricDate         = yesterday,
                        ActiveStaffCount   = activeStaffCount,
                        NewPatientsCount   = 0,
                        TotalPatientsCount = 0,
                        AppointmentsCount  = 0,
                        InvoicesCount      = 0,
                        StorageUsedGB      = 0,
                        LastAggregatedAt   = DateTimeOffset.UtcNow,
                    });
                }

                processed++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aggregating metrics for clinic {ClinicId}", clinic.Id);
                errors++;
            }
        }

        // Save all metric upserts in one round-trip
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usage metrics aggregation: {Processed} processed, {Errors} errors", processed, errors);
    }
}
