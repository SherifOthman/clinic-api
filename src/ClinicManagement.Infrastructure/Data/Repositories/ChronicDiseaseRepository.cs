using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class ChronicDiseaseRepository : IChronicDiseaseRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<ChronicDisease> _dbSet;

    public ChronicDiseaseRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<ChronicDisease>();
    }

    public async Task<ChronicDisease?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<ChronicDisease>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ChronicDisease>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(cd => cd.IsActive)
            .OrderBy(cd => cd.NameEn)
            .ToListAsync(cancellationToken);
    }

    public async Task<ChronicDisease> AddAsync(ChronicDisease entity, CancellationToken cancellationToken = default)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task<ChronicDisease> UpdateAsync(ChronicDisease entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        return Task.FromResult(entity);
    }

    public Task DeleteAsync(ChronicDisease entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(e => e.Id == id, cancellationToken);
    }
}