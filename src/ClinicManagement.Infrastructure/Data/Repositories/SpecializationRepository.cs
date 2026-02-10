using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class SpecializationRepository : BaseRepository<Specialization>, ISpecializationRepository
{
    public SpecializationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Specialization>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.NameEn)
            .ToListAsync(cancellationToken);
    }
}
