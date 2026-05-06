using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Audit.Queries;

/// <summary>
/// Returns audit logs for a specific clinic — used by Clinic Owners.
/// Reuses the same repository method as the SuperAdmin query but always
/// passes the owner's clinicId, so they can never see another clinic's data.
/// </summary>
public class GetClinicAuditLogsHandler
    : IRequestHandler<GetClinicAuditLogsQuery, Result<PaginatedResult<AuditLogDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetClinicAuditLogsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PaginatedResult<AuditLogDto>>> Handle(
        GetClinicAuditLogsQuery request, CancellationToken ct)
    {
        // Always scope to the owner's clinic — never trust caller-supplied clinic IDs
        var result = await _uow.AuditLogs.GetProjectedPageAsync(
            request.Filter,
            request.ClinicId,   // injected from JWT in the controller
            request.PageNumber,
            request.PageSize,
            ct);

        var dtos = result.Items.Select(a => new AuditLogDto(
            a.Id, a.Timestamp,
            a.ClinicId, null,   // clinic name not needed — owner knows their own clinic
            a.UserId, a.FullName, a.Username, a.UserEmail, a.UserRole, a.UserAgent,
            a.EntityType, a.EntityId, a.Action.ToString(), a.IpAddress, a.Changes
        )).ToList();

        return Result.Success(PaginatedResult<AuditLogDto>.Create(
            dtos, result.TotalCount, result.PageNumber, result.PageSize));
    }
}
