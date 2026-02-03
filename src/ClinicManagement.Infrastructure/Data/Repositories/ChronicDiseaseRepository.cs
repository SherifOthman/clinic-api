using ClinicManagement.Application.Common.Interfaces;
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
        // Since we removed IsActive, just return all chronic diseases
        return await GetAllAsync(cancellationToken);
    }

    public new async Task<ChronicDisease> AddAsync(ChronicDisease entity, CancellationToken cancellationToken = default)
    {
        await base.AddAsync(entity, cancellationToken);
        return entity;
    }

    public new async Task<ChronicDisease> UpdateAsync(ChronicDisease entity, CancellationToken cancellationToken = default)
    {
        await base.UpdateAsync(entity, cancellationToken);
        return entity;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(e => e.Id == id, cancellationToken);
    }
}