using ClinicManagement.Domain.Common.Models;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface ISearchableRepository<TEntity, TSearchRequest> : IRepository<TEntity>
    where TEntity : class, IEntity
    where TSearchRequest : PaginationRequest
{
    Task<PagedResult<TEntity>> SearchAsync(TSearchRequest searchRequest, CancellationToken cancellationToken = default);
}