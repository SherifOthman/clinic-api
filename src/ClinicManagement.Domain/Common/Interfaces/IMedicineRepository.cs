using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IMedicineRepository : IRepository<Medicine>
{
    Task<IEnumerable<Medicine>> GetByClinicBranchIdAsync(Guid clinicBranchId, CancellationToken cancellationToken = default);
    Task<PagedResult<Medicine>> GetByClinicBranchIdPagedAsync(Guid clinicBranchId, SearchablePaginationRequest request, CancellationToken cancellationToken = default);
    Task<Medicine?> GetByIdAndClinicBranchIdAsync(Guid id, Guid clinicBranchId, CancellationToken cancellationToken = default);
    Task<Medicine?> GetByNameAndClinicBranchAsync(string name, Guid clinicBranchId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Medicine>> GetLowStockMedicinesAsync(Guid clinicBranchId, int threshold = 10, CancellationToken cancellationToken = default);
}
