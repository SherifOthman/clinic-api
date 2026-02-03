using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class ClinicBranchRepository : BaseRepository<ClinicBranch>, IClinicBranchRepository
{
    public ClinicBranchRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ClinicBranch>> GetByClinicIdAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(cb => cb.ClinicId == clinicId)
            .OrderBy(cb => cb.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<ClinicBranch?> GetByIdAndClinicIdAsync(Guid id, Guid clinicId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(cb => cb.Id == id && cb.ClinicId == clinicId, cancellationToken);
    }
}
