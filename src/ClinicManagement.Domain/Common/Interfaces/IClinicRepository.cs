using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IClinicRepository : IRepository<Clinic>
{
    Task<Clinic?> GetByOwnerIdAsync(int ownerId, CancellationToken cancellationToken = default);
    Task<Clinic?> GetWithBranchesAsync(int clinicId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Clinic>> GetAllWithBranchesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Clinic>> GetAllWithSubscriptionsAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<Clinic> Clinics, int Total)> GetPaginatedWithSubscriptionsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
