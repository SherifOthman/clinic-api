using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Models.Filters;
using ClinicManagement.Application.Features.Staff.QueryModels;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

/// <summary>Replaces IStaffRepository — works with ClinicMember instead of Staff.</summary>
public interface IClinicMemberRepository : IRepository<ClinicMember>
{
    Task<ClinicMember?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<ClinicMember?> GetByUserIdIgnoreFiltersAsync(Guid userId, CancellationToken ct = default);
    Task<ClinicMember?> GetByUserIdWithClinicAsync(Guid userId, CancellationToken ct = default);
    Task<ClinicMember?> GetByIdWithDoctorInfoAsync(Guid id, CancellationToken ct = default);
    Task<int> CountActiveAsync(CancellationToken ct = default);
    Task<int> CountActiveIgnoreFiltersAsync(CancellationToken ct = default);

    Task<PaginatedResult<StaffListRow>> GetProjectedPageAsync(
        StaffFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);

    Task<StaffDetailRow?> GetDetailAsync(Guid memberId, CancellationToken ct = default);
    Task<List<StaffRoleRow>> GetRolesByUserIdsAsync(List<Guid> userIds, CancellationToken ct = default);
}
