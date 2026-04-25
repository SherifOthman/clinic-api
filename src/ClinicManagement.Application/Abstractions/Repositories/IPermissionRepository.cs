using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IPermissionRepository
{
    Task<List<Permission>> GetByMemberIdAsync(Guid memberId, CancellationToken ct = default);
    Task SetPermissionsAsync(Guid memberId, IEnumerable<Permission> permissions, CancellationToken ct = default);
    Task SeedDefaultsAsync(Guid memberId, ClinicMemberRole role, CancellationToken ct = default);

    /// <summary>
    /// Removes the cached permission set for a member.
    /// Call after any operation that changes a member's permissions or role.
    /// </summary>
    void InvalidateCache(Guid memberId);

    /// <summary>
    /// Returns the default permissions for a role from the database.
    /// Falls back to DefaultPermissions.cs if the table is empty (migration period).
    /// </summary>
    Task<List<Permission>> GetDefaultsForRoleAsync(ClinicMemberRole role, CancellationToken ct = default);

    /// <summary>
    /// Returns all role defaults grouped by role — for SuperAdmin management.
    /// </summary>
    Task<Dictionary<ClinicMemberRole, List<Permission>>> GetAllRoleDefaultsAsync(CancellationToken ct = default);

    /// <summary>
    /// Replaces the default permissions for a role. SuperAdmin only.
    /// </summary>
    Task SetDefaultsForRoleAsync(ClinicMemberRole role, IEnumerable<Permission> permissions, CancellationToken ct = default);

    /// <summary>
    /// Seeds the RoleDefaultPermissions table from DefaultPermissions.cs.
    /// Idempotent — safe to call on every startup.
    /// </summary>
    Task SeedRoleDefaultsAsync(CancellationToken ct = default);
}
