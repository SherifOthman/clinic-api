using ClinicManagement.Domain.Entities;
using ClinicManagement.Persistence.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Jobs;

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
        var yesterday      = DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddDays(-1).Date);
        var yesterdayStart = yesterday.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var yesterdayEnd   = yesterdayStart.AddDays(1);

        var activeClinics = await TenantGuard.AsSystemQuery(_db.Set<Clinic>())
            .Where(c => c.IsActive && !c.IsDeleted)
            .ToListAsync();

        _logger.LogInformation("Aggregating metrics for {Count} clinics for {Date}", activeClinics.Count, yesterday);

        var processed = 0; var errors = 0;

        foreach (var clinic in activeClinics)
        {
            try
            {
                var clinicId = clinic.Id;

                var activeStaff = await TenantGuard.AsSystemQuery(_db.Set<ClinicMember>())
                    .CountAsync(m => m.ClinicId == clinicId && m.IsActive && !m.IsDeleted);

                var newPatients = await TenantGuard.AsSystemQuery(_db.Set<Patient>())
                    .CountAsync(p => p.ClinicId == clinicId && !p.IsDeleted
                        && p.CreatedAt >= yesterdayStart && p.CreatedAt < yesterdayEnd);

                var totalPatients = await TenantGuard.AsSystemQuery(_db.Set<Patient>())
                    .CountAsync(p => p.ClinicId == clinicId && !p.IsDeleted);

                var appointments = await TenantGuard.AsSystemQuery(_db.Set<Appointment>())
                    .CountAsync(a => a.ClinicId == clinicId && !a.IsDeleted && a.Date == yesterday);

                var invoices = await TenantGuard.AsSystemQuery(_db.Set<Invoice>())
                    .CountAsync(i => i.ClinicId == clinicId
                        && i.CreatedAt >= yesterdayStart && i.CreatedAt < yesterdayEnd);

                var existing = await TenantGuard.AsSystemQuery(_db.Set<ClinicUsageMetrics>())
                    .FirstOrDefaultAsync(m => m.ClinicId == clinicId && m.MetricDate == yesterday);

                if (existing is not null)
                {
                    existing.ActiveStaffCount  = activeStaff;
                    existing.NewPatientsCount  = newPatients;
                    existing.TotalPatientsCount = totalPatients;
                    existing.AppointmentsCount = appointments;
                    existing.InvoicesCount     = invoices;
                    existing.LastAggregatedAt  = DateTimeOffset.UtcNow;
                }
                else
                {
                    _db.Set<ClinicUsageMetrics>().Add(new ClinicUsageMetrics
                    {
                        ClinicId            = clinicId,
                        MetricDate          = yesterday,
                        ActiveStaffCount    = activeStaff,
                        NewPatientsCount    = newPatients,
                        TotalPatientsCount  = totalPatients,
                        AppointmentsCount   = appointments,
                        InvoicesCount       = invoices,
                        StorageUsedGB       = 0, // file storage not tracked in DB
                        LastAggregatedAt    = DateTimeOffset.UtcNow,
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
        _logger.LogInformation("Usage metrics aggregation: {Processed} processed, {Errors} errors", processed, errors);
    }
}
