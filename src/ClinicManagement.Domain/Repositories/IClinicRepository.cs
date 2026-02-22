using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Repositories;

public interface IClinicRepository : IRepository<Clinic>
{
    Task<Clinic?> GetByOwnerUserIdAsync(int ownerUserId, CancellationToken cancellationToken = default);
}
