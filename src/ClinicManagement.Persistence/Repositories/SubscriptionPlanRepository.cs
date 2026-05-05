using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Persistence.Security;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class SubscriptionPlanRepository : Repository<SubscriptionPlan>, ISubscriptionPlanRepository
{
    public SubscriptionPlanRepository(ApplicationDbContext context) : base(context) { }

    public Task<int> CountActiveSubscribersAsync(Guid planId, CancellationToken ct = default)
        => TenantGuard.AsUnfilteredQuery(Context.Set<ClinicSubscription>())
            .CountAsync(s => s.SubscriptionPlanId == planId &&
                             (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial), ct);
}
