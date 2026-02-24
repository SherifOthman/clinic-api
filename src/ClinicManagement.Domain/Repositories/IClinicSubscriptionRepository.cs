using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Repositories;

public interface IClinicSubscriptionRepository : IRepository<ClinicSubscription>
{
    Task<ClinicSubscription?> GetActiveByClinicIdAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClinicSubscription>> GetExpiringSubscriptionsAsync(int daysBeforeExpiry, CancellationToken cancellationToken = default);
}
