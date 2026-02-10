using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IPatientRepository : IRepository<Patient>
{
    Task<Patient?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Patient?> GetByIdWithIncludesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Patient?> GetByIdForClinicAsync(Guid id, Guid clinicId, CancellationToken cancellationToken = default);
    Task<PagedResult<Patient>> GetPagedForClinicAsync(Guid clinicId, SearchablePaginationRequest request, CancellationToken cancellationToken = default);
    Task<int> GetCountForClinicByYearAsync(Guid clinicId, int year, CancellationToken cancellationToken = default);
    Task<PagedResult<Patient>> GetByClinicBranchIdPagedAsync(
        Guid clinicBranchId,
        string? searchTerm,
        int pageNumber,
        int pageSize,
        string? sortBy = null,
        string sortDirection = "desc",
        CancellationToken cancellationToken = default);
}
