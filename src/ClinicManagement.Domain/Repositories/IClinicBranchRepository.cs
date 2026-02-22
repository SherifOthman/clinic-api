using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Repositories;

public interface IClinicBranchRepository : IRepository<ClinicBranch>
{
    Task<IEnumerable<ClinicBranch>> GetByClinicIdAsync(int clinicId, CancellationToken cancellationToken = default);
}
