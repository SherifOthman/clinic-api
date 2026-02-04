using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class SpecializationRepository : BaseRepository<Specialization>, ISpecializationRepository
{
    public SpecializationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Specialization>> GetActiveSpecializationsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Specializations
            .Where(s => s.IsActive)
            .OrderBy(s => s.NameEn)
            .ToListAsync(cancellationToken);
    }

    public async Task<Specialization?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Specializations
            .FirstOrDefaultAsync(s => s.NameEn == name || s.NameAr == name, cancellationToken);
    }
}
