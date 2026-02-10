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
}
