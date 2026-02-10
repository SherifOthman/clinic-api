using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface ISpecializationRepository : IRepository<Specialization>
{
    Task<IEnumerable<Specialization>> GetActiveAsync(CancellationToken cancellationToken = default);
}
