using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    private readonly DbSet<Clinic> _clinics;

    public AuditLogRepository(ApplicationDbContext context) : base(context)
    {
        _clinics = context.Set<Clinic>();
    }

    public async Task<PaginatedResult<AuditLog>> GetProjectedPageAsync(
        string? entityType,
        string? entityId,
        AuditAction? action,
        string? userSearch,
        Guid? clinicId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (!string.IsNullOrWhiteSpace(entityId))
            query = query.Where(a => a.EntityId == entityId);

        if (action.HasValue)
            query = query.Where(a => a.Action == action.Value);

        if (!string.IsNullOrWhiteSpace(userSearch))
            query = query.Where(a =>
                (a.FullName  != null && a.FullName.StartsWith(userSearch))  ||
                (a.Username  != null && a.Username.StartsWith(userSearch))  ||
                (a.UserEmail != null && a.UserEmail.StartsWith(userSearch)));

        if (clinicId.HasValue)
            query = query.Where(a => a.ClinicId == clinicId.Value);

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        query = query.OrderByDescending(a => a.Timestamp);

        return await query.ToPagedAsync(pageNumber, pageSize, ct);
    }

    public async Task<Guid?> ResolveClinicIdAsync(string clinicSearch, CancellationToken ct = default)
    {
        if (Guid.TryParse(clinicSearch, out var clinicGuid))
            return clinicGuid;

        return await _clinics
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .Where(c => c.Name.StartsWith(clinicSearch))
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Dictionary<Guid, string>> GetClinicNamesByIdsAsync(
        List<Guid> clinicIds, CancellationToken ct = default)
        => await _clinics
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .Where(c => clinicIds.Contains(c.Id))
            .Select(c => new { c.Id, c.Name })
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);
}
