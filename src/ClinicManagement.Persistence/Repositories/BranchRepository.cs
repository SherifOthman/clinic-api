using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Features.Branches.QueryModels;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class BranchRepository : Repository<ClinicBranch>, IBranchRepository
{
    public BranchRepository(ApplicationDbContext context) : base(context) { }

    public async Task<List<BranchRow>> GetProjectedListAsync(CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(b => b.PhoneNumbers)
            .OrderByDescending(b => b.IsMainBranch)
            .ThenBy(b => b.Name)
            .Select(b => new BranchRow(
                b.Id, b.Name, b.AddressLine,
                b.CityNameEn, b.CityNameAr,
                b.StateNameEn, b.StateNameAr,
                b.IsMainBranch, b.IsActive,
                b.PhoneNumbers.Select(p => p.PhoneNumber).ToList()))
            .ToListAsync(ct);

    public async Task<Guid> GetMainBranchIdAsync(CancellationToken ct = default)
        => await DbSet
            .Where(b => b.IsMainBranch)
            .Select(b => b.Id)
            .FirstOrDefaultAsync(ct);

    public async Task<ClinicBranch?> GetByIdWithPhonesAsync(Guid id, CancellationToken ct = default)
        => await DbSet
            .Include(b => b.PhoneNumbers)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
}
