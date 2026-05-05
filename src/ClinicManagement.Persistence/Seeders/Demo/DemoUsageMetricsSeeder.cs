using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Seeds ClinicUsageMetrics for the past 30 days so the Usage page shows real data.
/// Values are set near the plan limits to trigger warning/critical notifications.
///
/// Starter plan limits: 200 patients/mo, 500 appts/mo, 200 invoices/mo, 3 staff
/// </summary>
public class DemoUsageMetricsSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DemoUsageMetricsSeeder> _logger;

    public DemoUsageMetricsSeeder(ApplicationDbContext db, ILogger<DemoUsageMetricsSeeder> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task SeedAsync(DemoClinicContext ctx)
    {
        var existing = await _db.Set<ClinicUsageMetrics>().IgnoreQueryFilters()
            .CountAsync(m => m.ClinicId == ctx.ClinicId);

        if (existing >= 5) { _logger.LogInformation("Usage metrics already seeded — skipping"); return; }

        var now   = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.Date);

        // Seed 30 days of metrics — values ramp up toward plan limits
        // to show warning/critical states on the usage page
        var metrics = new List<ClinicUsageMetrics>();

        for (int day = 29; day >= 0; day--)
        {
            var date     = today.AddDays(-day);
            var progress = (29 - day) / 29.0; // 0.0 → 1.0 over the month

            metrics.Add(new ClinicUsageMetrics
            {
                ClinicId            = ctx.ClinicId,
                MetricDate          = date,
                ActiveStaffCount    = 3,
                NewPatientsCount    = (int)(170 * progress),      // peaks at 170/200 = 85%
                TotalPatientsCount  = 50 + (int)(50 * progress),
                AppointmentsCount   = (int)(480 * progress),      // peaks at 480/500 = 96% (critical)
                InvoicesCount       = (int)(162 * progress),      // peaks at 162/200 = 81% (warning)
                StorageUsedGB       = (decimal)(1.66 * progress),
                LastAggregatedAt    = now.AddDays(-day),
            });
        }

        _db.Set<ClinicUsageMetrics>().AddRange(metrics);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} demo usage metric rows", metrics.Count);
    }
}
