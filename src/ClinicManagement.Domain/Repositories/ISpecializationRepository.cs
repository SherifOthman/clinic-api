using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Repositories;

public interface ISpecializationRepository
{
    Task<IEnumerable<Specialization>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Specialization?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
