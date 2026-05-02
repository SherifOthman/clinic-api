using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ClinicManagement.Persistence.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
    private static string MemberCacheKey(Guid memberId) => $"permissions:{memberId}";

    public PermissionRepository(ApplicationDbContext db, IMemoryCache cache)
    {
        _db    = db;
        _cache = cache;
    }

    public async Task<List<Permission>> GetByMemberIdAsync(
        Guid memberId, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(MemberCacheKey(memberId), out List<Permission>? cached))
            return cached!;

        var permissions = await _db.Set<ClinicMemberPermission>()
            .Where(p => p.ClinicMemberId == memberId)
            .Select(p => p.Permission)
            .ToListAsync(ct);

        _cache.Set(MemberCacheKey(memberId), permissions, CacheDuration);
        return permissions;
    }

    public async Task SetPermissionsAsync(
        Guid memberId, IEnumerable<Permission> permissions, CancellationToken ct = default)
    {
        // Load and remove existing — tracked delete stays within the UoW transaction
        var existing = await _db.Set<ClinicMemberPermission>()
            .Where(p => p.ClinicMemberId == memberId)
            .ToListAsync(ct);

        _db.Set<ClinicMemberPermission>().RemoveRange(existing);

        var rows = permissions.Select(p => new ClinicMemberPermission
        {
            ClinicMemberId = memberId,
            Permission     = p,
        });

        await _db.Set<ClinicMemberPermission>().AddRangeAsync(rows, ct);
        // Caller is responsible for SaveChangesAsync.
        InvalidateCache(memberId);
    }

    public async Task SeedDefaultsAsync(
        Guid memberId, ClinicMemberRole role, CancellationToken ct = default)
    {
        // Owner bypasses permission checks entirely in PermissionAuthorizationHandler —
        // seeding rows for them is wasted writes. Skip.
        if (role == ClinicMemberRole.Owner) return;

        var rows = DefaultPermissions.ForRole(role).Select(p => new ClinicMemberPermission
        {
            ClinicMemberId = memberId,
            Permission     = p,
        });
        await _db.Set<ClinicMemberPermission>().AddRangeAsync(rows, ct);
        // Caller is responsible for SaveChangesAsync.
    }

    public void InvalidateCache(Guid memberId)
        => _cache.Remove(MemberCacheKey(memberId));
}
