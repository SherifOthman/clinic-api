using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Staff.QueryModels;
using ClinicManagement.Application.Features.Staff.Queries;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IInvitationRepository : IRepository<StaffInvitation>
{
    Task<StaffInvitation?> GetByIdWithSpecializationAsync(Guid id, CancellationToken ct = default);
    Task<StaffInvitation?> GetByTokenAsync(string token, CancellationToken ct = default);

    /// <summary>Count pending (not accepted, not canceled, not expired) invitations.</summary>
    Task<int> CountPendingAsync(CancellationToken ct = default);

    Task<PaginatedResult<InvitationListRow>> GetProjectedPageAsync(
        InvitationStatus? status,
        string? role,
        string? sortBy,
        string? sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);

    Task<InvitationDetailRow?> GetDetailAsync(Guid id, CancellationToken ct = default);
}
