using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Staff.QueryModels;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IStaffRepository : IRepository<Staff>
{
    Task<Staff?> GetByIdWithDoctorProfileAsync(Guid id, CancellationToken ct = default);
    Task<Staff?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Staff?> GetByUserIdIgnoreFiltersAsync(Guid userId, CancellationToken ct = default);
    Task<int> CountActiveIgnoreFiltersAsync(CancellationToken ct = default);

    /// <summary>Count active staff in the current clinic.</summary>
    Task<int> CountActiveAsync(CancellationToken ct = default);

    Task<PaginatedResult<StaffListRow>> GetProjectedPageAsync(
        bool? isActive,
        string? role,
        string? sortBy,
        string? sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);

    Task<StaffDetailRow?> GetDetailAsync(Guid staffId, CancellationToken ct = default);
    Task<List<StaffRoleRow>> GetRolesByUserIdsAsync(List<Guid> userIds, CancellationToken ct = default);
}
