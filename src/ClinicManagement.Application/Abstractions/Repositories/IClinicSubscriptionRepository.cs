using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.SubscriptionPlans.QueryModels;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IClinicSubscriptionRepository : IRepository<ClinicSubscription>
{
    Task<ClinicSubscriptionRow?> GetLatestAsync(CancellationToken ct = default);
    Task<int> CountByStatusIgnoreFiltersAsync(SubscriptionStatus status, CancellationToken ct = default);

    /// <summary>Returns today's aggregated usage metrics for the clinic, or null if not yet run.</summary>
    Task<ClinicUsageMetrics?> GetTodayMetricsAsync(Guid clinicId, CancellationToken ct = default);

    /// <summary>Returns the active plan limits for the clinic, or null if no active subscription.</summary>
    Task<SubscriptionPlan?> GetActivePlanLimitsAsync(Guid clinicId, CancellationToken ct = default);
}
