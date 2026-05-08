using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Audit.Queries;

public class GetClinicAuditLogsHandler
    : IRequestHandler<GetClinicAuditLogsQuery, Result<PaginatedResult<AuditLogDto>>>
{
    private readonly IAuditLogRepository _auditLogs;

    public GetClinicAuditLogsHandler(IAuditLogRepository auditLogs) => _auditLogs = auditLogs;

    public async Task<Result<PaginatedResult<AuditLogDto>>> Handle(
        GetClinicAuditLogsQuery request, CancellationToken ct)
    {
        var result = await _auditLogs.GetProjectedPageAsync(
            request.Filter,
            request.ClinicId,
            request.PageNumber,
            request.PageSize,
            ct);

        var dtos = result.Items.Select(a => new AuditLogDto(
            a.Id, a.Timestamp,
            a.ClinicId, null,
            a.UserId, a.FullName, a.Username, a.UserEmail, a.UserRole, a.UserAgent,
            a.EntityType, a.EntityId, a.Action.ToString(), a.IpAddress, a.Changes
        )).ToList();

        return Result.Success(PaginatedResult<AuditLogDto>.Create(
            dtos, result.TotalCount, result.PageNumber, result.PageSize));
    }
}
