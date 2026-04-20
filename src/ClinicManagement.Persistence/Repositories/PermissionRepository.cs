using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly ApplicationDbContext _db;

    public PermissionRepository(ApplicationDbContext db) => _db = db;

    public async Task<List<Permission>> GetByMemberIdAsync(Guid memberId, CancellationToken ct = default)
        => await _db.Set<ClinicMemberPermission>()
            .Where(p => p.ClinicMemberId == memberId)
            .Select(p => p.Permission)
            .ToListAsync(ct);

    public async Task SetPermissionsAsync(Guid memberId, IEnumerable<Permission> permissions, CancellationToken ct = default)
    {
        // Delete all existing permissions for this member
        await _db.Set<ClinicMemberPermission>()
            .Where(p => p.ClinicMemberId == memberId)
            .ExecuteDeleteAsync(ct);

        // Insert the new set
        var rows = permissions.Select(p => new ClinicMemberPermission
        {
            ClinicMemberId = memberId,
            Permission     = p,
        });
        await _db.Set<ClinicMemberPermission>().AddRangeAsync(rows, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SeedDefaultsAsync(Guid memberId, ClinicMemberRole role, CancellationToken ct = default)
    {
        var defaults = DefaultPermissions.ForRole(role);
        var rows = defaults.Select(p => new ClinicMemberPermission
        {
            ClinicMemberId = memberId,
            Permission     = p,
        });
        await _db.Set<ClinicMemberPermission>().AddRangeAsync(rows, ct);
        // Caller is responsible for SaveChangesAsync
    }
}
