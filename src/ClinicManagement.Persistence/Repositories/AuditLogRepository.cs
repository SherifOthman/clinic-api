using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Models.Filters;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Persistence.Security;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    private readonly DbSet<Clinic> _clinics;

    public AuditLogRepository(ApplicationDbContext context) : base(context)
        => _clinics = context.Set<Clinic>();

    public async Task<PaginatedResult<AuditLog>> GetProjectedPageAsync(
        AuditLogFilter filter,
        Guid? resolvedClinicId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.EntityType))
            query = query.Where(a => a.EntityType == filter.EntityType);

        if (!string.IsNullOrWhiteSpace(filter.EntityId))
            query = query.Where(a => a.EntityId == filter.EntityId);

        if (filter.Action.HasValue)
            query = query.Where(a => a.Action == filter.Action.Value);

        if (!string.IsNullOrWhiteSpace(filter.UserSearch))
            query = query.Where(a =>
                (a.FullName  != null && a.FullName.StartsWith(filter.UserSearch))  ||
                (a.Username  != null && a.Username.StartsWith(filter.UserSearch))  ||
                (a.UserEmail != null && a.UserEmail.StartsWith(filter.UserSearch)));

        if (resolvedClinicId.HasValue)
            query = query.Where(a => a.ClinicId == resolvedClinicId.Value);

        if (filter.From.HasValue)
            query = query.Where(a => a.Timestamp >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(a => a.Timestamp <= filter.To.Value);

        query = query.OrderByDescending(a => a.Timestamp);

        return await query.ToPagedAsync(pageNumber, pageSize, ct);
    }

    public async Task<Guid?> ResolveClinicIdAsync(string clinicSearch, CancellationToken ct = default)
    {
        if (Guid.TryParse(clinicSearch, out var clinicGuid))
            return clinicGuid;

        return await TenantGuard.AsSystemQuery(_clinics)
            .Where(c => c.Name.StartsWith(clinicSearch))
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Dictionary<Guid, string>> GetClinicNamesByIdsAsync(
        List<Guid> clinicIds, CancellationToken ct = default)
        => await TenantGuard.AsSystemQuery(_clinics)
            .Where(c => clinicIds.Contains(c.Id))
            .Select(c => new { c.Id, c.Name })
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);
}
