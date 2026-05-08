using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Persistence.Security;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class ClinicRepository : Repository<Clinic>, IClinicRepository
{
    public ClinicRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Clinic?> GetByOwnerIdAsync(Guid ownerUserId, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(c => c.OwnerUserId == ownerUserId, ct);

    public async Task<bool> ExistsByOwnerIdAsync(Guid ownerUserId, CancellationToken ct = default)
        => await DbSet.AnyAsync(c => c.OwnerUserId == ownerUserId, ct);

    public async Task<Dictionary<Guid, string>> GetNamesByIdsAsync(List<Guid> ids, CancellationToken ct = default)
        => await TenantGuard.AsSystemQuery(DbSet)
            .Where(c => ids.Contains(c.Id))
            .Select(c => new { c.Id, c.Name })
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);

    public async Task<List<Guid>> FindIdsByNameAsync(string nameSearch, CancellationToken ct = default)
        => await TenantGuard.AsSystemQuery(DbSet)
            .Where(c => c.Name.StartsWith(nameSearch))
            .Select(c => c.Id)
            .ToListAsync(ct);

    public async Task<int> CountIgnoreFiltersAsync(CancellationToken ct = default)
        => await TenantGuard.AsSystemQuery(DbSet).CountAsync(ct);

    public async Task<string?> GetCountryCodeAsync(Guid clinicId, CancellationToken ct = default)
        => await TenantGuard.AsSystemQuery(DbSet)
            .Where(c => c.Id == clinicId)
            .Select(c => c.CountryCode)
            .FirstOrDefaultAsync(ct);
}
