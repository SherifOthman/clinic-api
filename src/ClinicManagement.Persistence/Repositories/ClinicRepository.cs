using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
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
        => await DbSet
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .Where(c => ids.Contains(c.Id))
            .Select(c => new { c.Id, c.Name })
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);

    public async Task<List<Guid>> FindIdsByNameAsync(string nameSearch, CancellationToken ct = default)
        => await DbSet
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .Where(c => c.Name.StartsWith(nameSearch))
            .Select(c => c.Id)
            .ToListAsync(ct);

    public async Task<int> CountIgnoreFiltersAsync(CancellationToken ct = default)
        => await DbSet
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .CountAsync(ct);
}
