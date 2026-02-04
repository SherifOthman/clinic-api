using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class ChronicDiseaseRepository : BaseRepository<ChronicDisease>, IChronicDiseaseRepository
{
    public ChronicDiseaseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ChronicDisease>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        // Since we removed IsActive, just return all chronic diseases with AsNoTracking
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<ChronicDisease>> GetActivePagedAsync(PaginationRequest request, CancellationToken cancellationToken = default)
    {
        // Since we removed IsActive, just return paginated results for all chronic diseases
        return await GetPagedAsync(request, cancellationToken);
    }

    public new async Task<ChronicDisease> AddAsync(ChronicDisease entity, CancellationToken cancellationToken = default)
    {
        await base.AddAsync(entity, cancellationToken);
        return entity;
    }

    public new void Update(ChronicDisease entity)
    {
        base.Update(entity);
    }

    public new void Delete(ChronicDisease entity)
    {
        base.Delete(entity);
    }
}
