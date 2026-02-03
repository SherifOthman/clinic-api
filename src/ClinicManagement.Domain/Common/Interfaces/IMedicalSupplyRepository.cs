using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IMedicalSupplyRepository : IRepository<MedicalSupply>
{
    Task<IEnumerable<MedicalSupply>> GetByClinicBranchIdAsync(Guid clinicBranchId, CancellationToken cancellationToken = default);
    Task<PagedResult<MedicalSupply>> GetByClinicBranchIdPagedAsync(Guid clinicBranchId, SearchablePaginationRequest request, CancellationToken cancellationToken = default);
    Task<MedicalSupply?> GetByIdAndClinicBranchIdAsync(Guid id, Guid clinicBranchId, CancellationToken cancellationToken = default);
    Task<MedicalSupply?> GetByNameAndClinicBranchAsync(string name, Guid clinicBranchId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MedicalSupply>> GetLowStockSuppliesAsync(Guid clinicBranchId, int threshold = 10, CancellationToken cancellationToken = default);
}
