using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface ISpecializationRepository : IRepository<Specialization>
{
    Task<IEnumerable<Specialization>> GetActiveSpecializationsAsync(CancellationToken cancellationToken = default);
    Task<Specialization?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
