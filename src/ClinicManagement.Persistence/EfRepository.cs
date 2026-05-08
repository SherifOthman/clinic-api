using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence;

/// <summary>
/// EF Core convenience base for repositories that implement IRepository&lt;T&gt;.
///
/// This is an EF-specific implementation detail — it is NOT the Repository pattern itself.
/// The pattern lives in IRepository&lt;T&gt; (Application layer, ORM-agnostic).
///
/// Concrete repositories that need a different data access strategy (Dapper, ADO.NET, etc.)
/// should implement IRepository&lt;T&gt; directly without extending this class.
/// Swapping is a one-line change in DependencyInjection.cs.
/// </summary>
public abstract class EfRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    protected EfRepository(ApplicationDbContext context)
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
