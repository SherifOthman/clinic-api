using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class StateRepository : BaseRepository<State>, IStateRepository
{
    public StateRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<State?> GetByGeonameIdAsync(int geonameId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.GeonameId == geonameId, cancellationToken);
    }
}
