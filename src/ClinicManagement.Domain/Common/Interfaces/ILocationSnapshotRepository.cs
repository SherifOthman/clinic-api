using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface ILocationSnapshotRepository : IRepository<LocationSnapshot>
{
    Task<LocationSnapshot?> GetByGeoNameIdAsync(int geoNameId, CancellationToken cancellationToken = default);
    Task<List<LocationSnapshot>> GetByGeoNameIdsAsync(List<int> geoNameIds, CancellationToken cancellationToken = default);
}
