using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class CountryRepository : BaseRepository<Country>, ICountryRepository
{
    public CountryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Country?> GetByGeonameIdAsync(int geonameId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.GeonameId == geonameId, cancellationToken);
    }
}
