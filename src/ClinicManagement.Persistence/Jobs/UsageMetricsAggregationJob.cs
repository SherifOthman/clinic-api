using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Persistence.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Jobs;

/// <summary>
/// Runs daily and updates the current month's usage row for each active clinic.
///
/// The row represents month-to-date totals so the clinic owner can see
/// "you've used X of Y this month" and get notified when approaching limits.
///
/// Monthly counters (patients, appointments, invoices) are cumulative from
/// the 1st of the current month to now — matching the plan's monthly limits.
///
/// Absolute counters (staff, branches) reflect the current state regardless of month.
/// </summary>
public class UsageMetricsAggregationJob
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<UsageMetricsAggregationJob> _logger;

    public UsageMetricsAggregationJob(ApplicationDbContext db, ILogger<UsageMetricsAggregationJob> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        var now          = DateTimeOffset.UtcNow;
        var today        = DateOnly.FromDateTime(now.Date);
        var monthStart   = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var monthStartDate = DateOnly.FromDateTime(monthStart.Date);

        var activeClinics = await TenantGuard.AsSystemQuery(_db.Set<Clinic>())
            .Where(c => c.IsActive && !c.IsDeleted)
            .ToListAsync();

        _logger.LogInformation("Aggregating usage metrics for {Count} clinics ({Month:yyyy-MM})",
            activeClinics.Count, now);

        var processed = 0; var errors = 0;

        foreach (var clinic in activeClinics)
        {
            try
            {
                var clinicId = clinic.Id;

                // ── Absolute limits (current state) ───────────────────────────
                var activeStaff = await TenantGuard.AsSystemQuery(_db.Set<ClinicMember>())
                    .CountAsync(m => m.ClinicId == clinicId && m.IsActive && !m.IsDeleted);

                var activeBranches = await TenantGuard.AsSystemQuery(_db.Set<ClinicBranch>())
                    .CountAsync(b => b.ClinicId == clinicId && b.IsActive && !b.IsDeleted);

                var totalPatients = await TenantGuard.AsSystemQuery(_db.Set<Patient>())
                    .CountAsync(p => p.ClinicId == clinicId && !p.IsDeleted);

                // ── Monthly limits (month-to-date) ────────────────────────────
                var newPatientsThisMonth = await TenantGuard.AsSystemQuery(_db.Set<Patient>())
                    .CountAsync(p => p.ClinicId == clinicId && !p.IsDeleted
                        && p.CreatedAt >= monthStart);

                var appointmentsThisMonth = await TenantGuard.AsSystemQuery(_db.Set<Appointment>())
                    .CountAsync(a => a.ClinicId == clinicId && !a.IsDeleted
                        && a.Date >= monthStartDate);

                var invoicesThisMonth = await TenantGuard.AsSystemQuery(_db.Set<Invoice>())
                    .CountAsync(i => i.ClinicId == clinicId
                        && i.CreatedAt >= monthStart);

                // ── Upsert today's row ────────────────────────────────────────
                // One row per clinic per day — today's row is the "live" snapshot
                // the owner sees. Historical rows let us chart usage over time.
                var existing = await TenantGuard.AsSystemQuery(_db.Set<ClinicUsageMetrics>())
                    .FirstOrDefaultAsync(m => m.ClinicId == clinicId && m.MetricDate == today);

                if (existing is not null)
                {
                    existing.ActiveStaffCount   = activeStaff;
                    existing.NewPatientsCount   = newPatientsThisMonth;
                    existing.TotalPatientsCount = totalPatients;
                    existing.AppointmentsCount  = appointmentsThisMonth;
                    existing.InvoicesCount      = invoicesThisMonth;
                    existing.LastAggregatedAt   = now;
                }
                else
                {
                    _db.Set<ClinicUsageMetrics>().Add(new ClinicUsageMetrics
                    {
                        ClinicId            = clinicId,
                        MetricDate          = today,
                        ActiveStaffCount    = activeStaff,
                        NewPatientsCount    = newPatientsThisMonth,
                        TotalPatientsCount  = totalPatients,
                        AppointmentsCount   = appointmentsThisMonth,
                        InvoicesCount       = invoicesThisMonth,
                        StorageUsedGB       = 0, // file storage not tracked in DB
                        LastAggregatedAt    = now,
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

        await _db.SaveChangesAsync();
        _logger.LogInformation("Usage metrics aggregation complete: {Processed} processed, {Errors} errors",
            processed, errors);
    }
}
