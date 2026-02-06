using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface ICountryRepository : IRepository<Country>
{
    Task<Country?> GetByGeonameIdAsync(int geonameId, CancellationToken cancellationToken = default);
}
