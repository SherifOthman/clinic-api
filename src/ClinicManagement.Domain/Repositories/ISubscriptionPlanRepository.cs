using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Repositories;

public interface ISubscriptionPlanRepository : IRepository<SubscriptionPlan>
{
    Task<IEnumerable<SubscriptionPlan>> GetActiveAsync(CancellationToken cancellationToken = default);
}
