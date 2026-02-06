using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface ICityRepository : IRepository<City>
{
    Task<City?> GetByGeonameIdAsync(int geonameId, CancellationToken cancellationToken = default);
}
