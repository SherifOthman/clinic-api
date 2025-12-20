using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class ClinicRepository : Repository<Clinic>, IClinicRepository
{
    public ClinicRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Clinic?> GetByOwnerIdAsync(int ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Owner)
            .FirstOrDefaultAsync(c => c.OwnerId == ownerId, cancellationToken);
    }

    public async Task<Clinic?> GetWithBranchesAsync(int clinicId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Owner)
            .FirstOrDefaultAsync(c => c.Id == clinicId, cancellationToken);
    }

    public async Task<IEnumerable<Clinic>> GetAllWithBranchesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Owner)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Clinic>> GetAllWithSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Clinic> Clinics, int Total)> GetPaginatedWithSubscriptionsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.OrderByDescending(c => c.CreatedAt);
        
        var total = await query.CountAsync(cancellationToken);
        var clinics = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (clinics, total);
    }
}
