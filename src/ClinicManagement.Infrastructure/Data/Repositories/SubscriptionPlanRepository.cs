using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class SubscriptionPlanRepository : BaseRepository<SubscriptionPlan>, ISubscriptionPlanRepository
{
    public SubscriptionPlanRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SubscriptionPlan>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(sp => sp.IsActive)
            .OrderBy(sp => sp.DisplayOrder)
            .ToListAsync(cancellationToken);
    }
}
