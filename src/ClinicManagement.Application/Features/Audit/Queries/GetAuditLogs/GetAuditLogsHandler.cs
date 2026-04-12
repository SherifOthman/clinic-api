using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Audit.Queries;

public class GetAuditLogsHandler : IRequestHandler<GetAuditLogsQuery, Result<PaginatedResult<AuditLogDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetAuditLogsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PaginatedResult<AuditLogDto>>> Handle(
        GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        Guid? clinicId = null;
        if (!string.IsNullOrWhiteSpace(request.ClinicSearch))
            clinicId = await _uow.AuditLogs.ResolveClinicIdAsync(request.ClinicSearch, cancellationToken);

        var result = await _uow.AuditLogs.GetProjectedPageAsync(
            request.EntityType,
            request.EntityId,
            request.Action,
            request.UserSearch,
            clinicId,
            request.From,
            request.To,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var clinicIds = result.Items
            .Where(a => a.ClinicId.HasValue)
            .Select(a => a.ClinicId!.Value)
            .Distinct().ToList();

        var clinicNames = clinicIds.Count > 0
            ? await _uow.AuditLogs.GetClinicNamesByIdsAsync(clinicIds, cancellationToken)
            : new Dictionary<Guid, string>();

        var dtos = result.Items.Select(a => new AuditLogDto(
            a.Id, a.Timestamp, a.ClinicId,
            a.ClinicId.HasValue && clinicNames.TryGetValue(a.ClinicId.Value, out var cn) ? cn : null,
            a.UserId, a.FullName, a.Username, a.UserEmail, a.UserRole, a.UserAgent,
            a.EntityType, a.EntityId, a.Action.ToString(), a.IpAddress, a.Changes
        )).ToList();

        return Result.Success(PaginatedResult<AuditLogDto>.Create(dtos, result.TotalCount, result.PageNumber, result.PageSize));
    }
}
