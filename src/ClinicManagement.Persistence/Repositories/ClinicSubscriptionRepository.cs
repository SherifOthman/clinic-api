using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Features.SubscriptionPlans.QueryModels;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Persistence.Security;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class ClinicSubscriptionRepository : Repository<ClinicSubscription>, IClinicSubscriptionRepository
{
    public ClinicSubscriptionRepository(ApplicationDbContext context) : base(context) { }

    public async Task<ClinicSubscriptionRow?> GetLatestAsync(CancellationToken ct = default)
        => await DbSet
            .OrderByDescending(s => s.StartDate)
            .Select(s => new ClinicSubscriptionRow(
                s.Status,
                s.TrialEndDate,
                s.EndDate,
                s.SubscriptionPlan.Name))
            .FirstOrDefaultAsync(ct);

    public async Task<int> CountByStatusIgnoreFiltersAsync(SubscriptionStatus status, CancellationToken ct = default)
        => await TenantGuard.AsSystemQuery(DbSet)
            .CountAsync(s => s.Status == status, ct);

    public async Task<ClinicUsageMetrics?> GetTodayMetricsAsync(Guid clinicId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);
        return await Context.Set<ClinicUsageMetrics>()
            .FirstOrDefaultAsync(m => m.ClinicId == clinicId && m.MetricDate == today, ct);
    }

    public async Task<SubscriptionPlan?> GetActivePlanLimitsAsync(Guid clinicId, CancellationToken ct = default)
        => await DbSet
            .Where(s => s.ClinicId == clinicId &&
                        (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial))
            .OrderByDescending(s => s.StartDate)
            .Select(s => s.SubscriptionPlan)
            .FirstOrDefaultAsync(ct);
}
