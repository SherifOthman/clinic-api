using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IClinicBranchRepository : IRepository<ClinicBranch>
{
    Task<IEnumerable<ClinicBranch>> GetByClinicIdAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<ClinicBranch?> GetByIdAndClinicIdAsync(Guid id, Guid clinicId, CancellationToken cancellationToken = default);
}
