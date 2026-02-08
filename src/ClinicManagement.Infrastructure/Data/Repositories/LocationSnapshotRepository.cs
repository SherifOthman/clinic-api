using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class LocationSnapshotRepository : BaseRepository<LocationSnapshot>, ILocationSnapshotRepository
{
    public LocationSnapshotRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<LocationSnapshot?> GetByGeoNameIdAsync(int geoNameId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(l => l.GeoNameId == geoNameId, cancellationToken);
    }

    public async Task<List<LocationSnapshot>> GetByGeoNameIdsAsync(List<int> geoNameIds, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(l => geoNameIds.Contains(l.GeoNameId))
            .ToListAsync(cancellationToken);
    }
}
