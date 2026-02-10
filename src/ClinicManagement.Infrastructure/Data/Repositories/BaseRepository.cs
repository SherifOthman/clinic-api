using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task<PagedResult<T>> GetPagedAsync(PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, request.PageNumber, request.PageSize);
    }

    /// <summary>
    /// Virtual method for searchable pagination - override in derived repositories
    /// </summary>
    public virtual async Task<PagedResult<T>> GetPagedAsync(SearchablePaginationRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking();

        // Apply search and filtering in derived classes
        query = ApplySearchAndFilters(query, request);

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting in derived classes
        query = ApplySorting(query, request);

        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, request.PageNumber, request.PageSize);
    }

    /// <summary>
    /// Override this method in derived repositories to implement search and filtering
    /// </summary>
    protected virtual IQueryable<T> ApplySearchAndFilters(IQueryable<T> query, SearchablePaginationRequest request)
    {
        return query;
    }

    /// <summary>
    /// Override this method in derived repositories to implement custom sorting
    /// </summary>
    protected virtual IQueryable<T> ApplySorting(IQueryable<T> query, SearchablePaginationRequest request)
    {
        return query;
    }

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken) != null;
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual void DeleteRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }
}
