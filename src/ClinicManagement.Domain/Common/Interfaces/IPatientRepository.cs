using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IPatientRepository : IRepository<Patient>
{
    Task<Patient?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<Patient>> GetByClinicBranchIdPagedAsync(
        Guid clinicBranchId,
        string? searchTerm,
        int pageNumber,
        int pageSize,
        string? sortBy = null,
        string sortDirection = "desc",
        CancellationToken cancellationToken = default);
}
