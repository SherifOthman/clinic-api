using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IMedicineRepository : IRepository<Medicine>
{
    Task<PagedResult<Medicine>> GetByClinicBranchIdPagedAsync(
        Guid clinicBranchId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<PagedResult<Medicine>> GetPagedByClinicBranchAsync(Guid clinicBranchId, PaginationRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<Medicine>> GetByClinicBranchAsync(Guid clinicBranchId, CancellationToken cancellationToken = default);
    Task<Medicine?> GetByNameAndClinicBranchAsync(string name, Guid clinicBranchId, CancellationToken cancellationToken = default);
}
