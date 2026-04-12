using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.Branches.QueryModels;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IBranchRepository : IRepository<ClinicBranch>
{
    Task<List<BranchRow>> GetProjectedListAsync(CancellationToken ct = default);
    Task<Guid> GetMainBranchIdAsync(CancellationToken ct = default);
    Task<ClinicBranch?> GetByIdWithPhonesAsync(Guid id, CancellationToken ct = default);
}
