using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IClinicRepository : IRepository<Clinic>
{
    Task<IEnumerable<Clinic>> GetByOwnerIdAsync(int ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Clinic>> GetActiveClinicsAsync(CancellationToken cancellationToken = default);
    Task<bool> IsOwnerOfClinicAsync(int userId, int clinicId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Clinic>> GetClinicsBySubscriptionPlanAsync(int subscriptionPlanId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Clinic>> GetClinicsPagedAsync(int? ownerId, bool? isActive, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
