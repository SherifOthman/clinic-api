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

    // Cache key format — memberId already encodes the clinic context (multi-tenant safe)
    private static string MemberCacheKey(Guid memberId) => $"permissions:{memberId}";

    // Separate cache for role defaults — shared across all members of the same role
    private static string RoleCacheKey(ClinicMemberRole role) => $"role-defaults:{role}";

    public PermissionRepository(ApplicationDbContext db, IMemoryCache cache)
    {
        _db    = db;
        _cache = cache;
    }

    // ── Member permissions ────────────────────────────────────────────────────

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
        await _db.Set<ClinicMemberPermission>()
            .Where(p => p.ClinicMemberId == memberId)
            .ExecuteDeleteAsync(ct);

        var rows = permissions.Select(p => new ClinicMemberPermission
        {
            ClinicMemberId = memberId,
            Permission     = p,
        });
        await _db.Set<ClinicMemberPermission>().AddRangeAsync(rows, ct);
        await _db.SaveChangesAsync(ct);

        InvalidateCache(memberId);
    }

    public async Task SeedDefaultsAsync(
        Guid memberId, ClinicMemberRole role, CancellationToken ct = default)
    {
        // Use DB-driven defaults if available, fall back to code constants
        var defaults = await GetDefaultsForRoleAsync(role, ct);

        var rows = defaults.Select(p => new ClinicMemberPermission
        {
            ClinicMemberId = memberId,
            Permission     = p,
        });
        await _db.Set<ClinicMemberPermission>().AddRangeAsync(rows, ct);
        // Caller is responsible for SaveChangesAsync.
        // No cache invalidation — member is new, no stale entry exists.
    }

    public void InvalidateCache(Guid memberId)
        => _cache.Remove(MemberCacheKey(memberId));

    // ── Role default permissions ──────────────────────────────────────────────

    public async Task<List<Permission>> GetDefaultsForRoleAsync(
        ClinicMemberRole role, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(RoleCacheKey(role), out List<Permission>? cached))
            return cached!;

        var dbDefaults = await _db.Set<RoleDefaultPermission>()
            .Where(r => r.Role == role)
            .Select(r => r.Permission)
            .ToListAsync(ct);

        // Fallback to code constants during migration period
        var result = dbDefaults.Count > 0
            ? dbDefaults
            : DefaultPermissions.ForRole(role).ToList();

        // Role defaults are stable — cache longer than member permissions
        _cache.Set(RoleCacheKey(role), result, TimeSpan.FromHours(1));
        return result;
    }

    public async Task SeedRoleDefaultsAsync(CancellationToken ct = default)
    {
        // Idempotent — skip if already seeded
        if (await _db.Set<RoleDefaultPermission>().AnyAsync(ct))
            return;

        var roles = Enum.GetValues<ClinicMemberRole>();
        var rows  = new List<RoleDefaultPermission>();

        foreach (var role in roles)
        {
            var defaults = DefaultPermissions.ForRole(role);
            rows.AddRange(defaults.Select(p => new RoleDefaultPermission
            {
                Role       = role,
                Permission = p,
            }));
        }

        if (rows.Count > 0)
        {
            await _db.Set<RoleDefaultPermission>().AddRangeAsync(rows, ct);
            await _db.SaveChangesAsync(ct);
        }

        // Invalidate role caches so next read picks up DB values
        foreach (var role in roles)
            _cache.Remove(RoleCacheKey(role));
    }
}
