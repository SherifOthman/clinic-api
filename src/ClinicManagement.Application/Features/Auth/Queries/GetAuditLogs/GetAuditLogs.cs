using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Auth.Queries;

public record GetAuditLogsQuery(
    Guid? ClinicId = null,
    Guid? UserId = null,
    string? EntityType = null,
    string? EntityId = null,
    AuditAction? Action = null,
    DateTime? From = null,
    DateTime? To = null,
    int PageNumber = 1,
    int PageSize = 50
) : IRequest<Result<AuditLogsResponse>>;

public record AuditLogDto(
    Guid Id,
    string Timestamp,
    Guid? ClinicId,
    string? ClinicName,
    Guid? UserId,
    string? UserName,
    string? UserLogin,
    string? UserRole,
    string EntityType,
    string EntityId,
    string Action,
    string? IpAddress,
    string? Changes
);

public record AuditLogsResponse(
    List<AuditLogDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

public class GetAuditLogsHandler : IRequestHandler<GetAuditLogsQuery, Result<AuditLogsResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetAuditLogsHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<AuditLogsResponse>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (request.ClinicId.HasValue)
            query = query.Where(a => a.ClinicId == request.ClinicId.Value);

        if (request.UserId.HasValue)
            query = query.Where(a => a.UserId == request.UserId.Value);

        if (!string.IsNullOrWhiteSpace(request.EntityType))
            query = query.Where(a => a.EntityType == request.EntityType);

        if (!string.IsNullOrWhiteSpace(request.EntityId))
            query = query.Where(a => a.EntityId == request.EntityId);

        if (request.Action.HasValue)
            query = query.Where(a => a.Action == request.Action.Value);

        if (request.From.HasValue)
            query = query.Where(a => a.Timestamp >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(a => a.Timestamp <= request.To.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var rawItems = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Resolve clinic names in one batch
        var clinicIds = rawItems
            .Where(a => a.ClinicId.HasValue)
            .Select(a => a.ClinicId!.Value)
            .Distinct()
            .ToList();

        var clinicNames = await _context.Clinics
            .IgnoreQueryFilters([Domain.Common.Constants.QueryFilterNames.Tenant])
            .Where(c => clinicIds.Contains(c.Id))
            .Select(c => new { c.Id, c.Name })
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        var items = rawItems.Select(a => new AuditLogDto(
            a.Id,
            a.Timestamp.ToString("O"),
            a.ClinicId,
            a.ClinicId.HasValue && clinicNames.TryGetValue(a.ClinicId.Value, out var name) ? name : null,
            a.UserId,
            a.UserName,
            a.UserLogin,
            a.UserRole,
            a.EntityType,
            a.EntityId,
            a.Action.ToString(),
            a.IpAddress,
            a.Changes
        )).ToList();

        return Result.Success(new AuditLogsResponse(
            items,
            totalCount,
            request.PageNumber,
            request.PageSize,
            (int)Math.Ceiling(totalCount / (double)request.PageSize)
        ));
    }
}
