using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence;

/// <summary>
/// EF Core base class for repositories that implement IRepository&lt;T&gt;.
/// Provides the five standard CRUD operations backed by a DbSet.
/// All query logic belongs in the concrete child repositories.
/// </summary>
public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    protected Repository(ApplicationDbContext context)
    {
        Context = context;
        DbSet   = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await DbSet.FindAsync([id], ct);

    public virtual async Task<int> CountAsync(CancellationToken ct = default)
        => await DbSet.CountAsync(ct);

    public async Task AddAsync(TEntity entity, CancellationToken ct = default)
        => await DbSet.AddAsync(entity, ct);

    public void Update(TEntity entity) => DbSet.Update(entity);
    public void Delete(TEntity entity) => DbSet.Remove(entity);
}
