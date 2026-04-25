using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IPermissionRepository
{
    Task<List<Permission>> GetByMemberIdAsync(Guid memberId, CancellationToken ct = default);
    Task SetPermissionsAsync(Guid memberId, IEnumerable<Permission> permissions, CancellationToken ct = default);
    Task SeedDefaultsAsync(Guid memberId, ClinicMemberRole role, CancellationToken ct = default);
    void InvalidateCache(Guid memberId);
}
