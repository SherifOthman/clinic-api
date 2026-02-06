using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class CityRepository : BaseRepository<City>, ICityRepository
{
    public CityRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<City?> GetByGeonameIdAsync(int geonameId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.GeonameId == geonameId, cancellationToken);
    }
}
