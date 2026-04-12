using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.SubscriptionPlans.QueryModels;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IClinicSubscriptionRepository : IRepository<ClinicSubscription>
{
    Task<ClinicSubscriptionRow?> GetLatestAsync(CancellationToken ct = default);
    Task<int> CountByStatusIgnoreFiltersAsync(SubscriptionStatus status, CancellationToken ct = default);
}
