using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class ClinicRepository : Repository<Clinic>, IClinicRepository
{
    public ClinicRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Clinic>> GetByOwnerIdAsync(int ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.OwnerId == ownerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Clinic>> GetActiveClinicsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsOwnerOfClinicAsync(int userId, int clinicId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(c => c.Id == clinicId && c.OwnerId == userId, cancellationToken);
    }

    public async Task<IEnumerable<Clinic>> GetClinicsBySubscriptionPlanAsync(int subscriptionPlanId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.SubscriptionPlanId == subscriptionPlanId)
            .ToListAsync(cancellationToken);
    }
}

