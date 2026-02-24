using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Repositories;

public interface IStaffBranchRepository : IRepository<StaffBranch>
{
    Task<IEnumerable<StaffBranch>> GetByStaffIdAsync(Guid staffId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StaffBranch>> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<StaffBranch?> GetPrimaryBranchAsync(Guid staffId, CancellationToken cancellationToken = default);
}
