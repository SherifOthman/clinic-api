using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Features.SubscriptionPlans.QueryModels;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
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
        => await DbSet
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .CountAsync(s => s.Status == status, ct);
}
