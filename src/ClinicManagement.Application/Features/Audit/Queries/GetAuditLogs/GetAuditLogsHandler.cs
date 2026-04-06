using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Audit.Queries;

public class GetAuditLogsHandler : IRequestHandler<GetAuditLogsQuery, Result<PaginatedResult<AuditLogDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAuditLogsHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<PaginatedResult<AuditLogDto>>> Handle(
        GetAuditLogsQuery request,
        CancellationToken cancellationToken)
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

        if (!string.IsNullOrWhiteSpace(request.UserSearch))
            query = query.Where(a =>
                (a.FullName  != null && a.FullName.Contains(request.UserSearch))  ||
                (a.Username  != null && a.Username.Contains(request.UserSearch))  ||
                (a.UserEmail != null && a.UserEmail.Contains(request.UserSearch)));

        if (!string.IsNullOrWhiteSpace(request.ClinicSearch))
        {
            if (Guid.TryParse(request.ClinicSearch, out var clinicGuid))
            {
                query = query.Where(a => a.ClinicId == clinicGuid);
            }
            else
            {
                var matchingClinicIds = await _context.Clinics
                    .IgnoreQueryFilters([QueryFilterNames.Tenant])
                    .Where(c => c.Name.Contains(request.ClinicSearch))
                    .Select(c => c.Id)
                    .ToListAsync(cancellationToken);

                query = query.Where(a => a.ClinicId.HasValue && matchingClinicIds.Contains(a.ClinicId.Value));
            }
        }

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

        // Resolve clinic names in a single query
        var clinicIds = rawItems
            .Where(a => a.ClinicId.HasValue)
            .Select(a => a.ClinicId!.Value)
            .Distinct()
            .ToList();

        var clinicNames = clinicIds.Count > 0
            ? await _context.Clinics
                .IgnoreQueryFilters([QueryFilterNames.Tenant])
                .Where(c => clinicIds.Contains(c.Id))
                .Select(c => new { c.Id, c.Name })
                .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken)
            : new Dictionary<Guid, string>();

        var items = rawItems.Select(a => new AuditLogDto(
            a.Id,
            a.Timestamp,
            a.ClinicId,
            a.ClinicId.HasValue && clinicNames.TryGetValue(a.ClinicId.Value, out var clinicName) ? clinicName : null,
            a.UserId,
            a.FullName,
            a.Username,
            a.UserEmail,
            a.UserRole,
            a.UserAgent,
            a.EntityType,
            a.EntityId,
            a.Action.ToString(),
            a.IpAddress,
            a.Changes
        )).ToList();

        return Result.Success(PaginatedResult<AuditLogDto>.Create(items, totalCount, request.PageNumber, request.PageSize));
    }
}
