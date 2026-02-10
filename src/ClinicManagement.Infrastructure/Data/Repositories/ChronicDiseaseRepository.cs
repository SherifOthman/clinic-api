using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class ChronicDiseaseRepository : BaseRepository<ChronicDisease>, IChronicDiseaseRepository
{
    public ChronicDiseaseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ChronicDisease>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .OrderBy(cd => cd.NameEn)
            .ToListAsync(cancellationToken);
    }
}
