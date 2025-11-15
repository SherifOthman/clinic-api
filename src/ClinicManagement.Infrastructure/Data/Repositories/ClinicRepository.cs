using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Infrastructure.Extensions;
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

    public async Task<IEnumerable<Clinic>> GetClinicsPagedAsync(int? ownerId, bool? isActive, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (ownerId.HasValue)
        {
            query = query.Where(c => c.OwnerId == ownerId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(c => c.Name)
            .Paginate(pageNumber, pageSize)
            .ToListAsync(cancellationToken);
    }
}

