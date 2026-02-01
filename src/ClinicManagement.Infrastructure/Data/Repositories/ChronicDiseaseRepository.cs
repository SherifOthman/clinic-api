using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class ChronicDiseaseRepository : BaseRepository<ChronicDisease>, IChronicDiseaseRepository
{
    public ChronicDiseaseRepository(ApplicationDbContext context, ICurrentUserService currentUserService) : base(context)
    {
    }

    public async Task<IEnumerable<ChronicDisease>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(cd => cd.IsActive)
            .OrderBy(cd => cd.NameEn)
            .ToListAsync(cancellationToken);
    }
}