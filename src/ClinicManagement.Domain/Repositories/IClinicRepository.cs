using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Repositories;

public interface IClinicRepository : IRepository<Clinic>
{
    Task<Clinic?> GetByOwnerUserIdAsync(Guid ownerUserId, CancellationToken cancellationToken = default);
}
