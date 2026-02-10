using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IMedicalSupplyRepository : IRepository<MedicalSupply>
{
    Task<PagedResult<MedicalSupply>> GetByClinicBranchIdPagedAsync(
        Guid clinicBranchId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
