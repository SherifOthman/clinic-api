using ClinicManagement.Domain.Common.Models;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IRepository<T> where T : class
{
    // Read operations
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<T>> GetPagedAsync(PaginationRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<T>> GetPagedAsync(SearchablePaginationRequest request, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    // Write operations
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);
    void Delete(T entity);
    void DeleteRange(IEnumerable<T> entities);
}
