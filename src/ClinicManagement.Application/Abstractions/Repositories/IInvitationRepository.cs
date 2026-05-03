using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Models.Filters;
using ClinicManagement.Application.Features.Staff.QueryModels;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IInvitationRepository : IRepository<StaffInvitation>
{
    Task<StaffInvitation?> GetByIdWithSpecializationAsync(Guid id, CancellationToken ct = default);
    Task<StaffInvitation?> GetByTokenAsync(string token, CancellationToken ct = default);

    /// <summary>Count pending (not accepted, not canceled, not expired) invitations.</summary>
    Task<int> CountPendingAsync(CancellationToken ct = default);

    Task<PaginatedResult<InvitationListRow>> GetProjectedPageAsync(
        InvitationFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);

    Task<InvitationDetailRow?> GetDetailAsync(Guid id, CancellationToken ct = default);
}
