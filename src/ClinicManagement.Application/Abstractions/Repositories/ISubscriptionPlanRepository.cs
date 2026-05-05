using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface ISubscriptionPlanRepository : IRepository<SubscriptionPlan>
{
    /// <summary>Count of clinics with an active/trial subscription on this plan. Used to block deactivation.</summary>
    Task<int> CountActiveSubscribersAsync(Guid planId, CancellationToken ct = default);
}
