using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IChronicDiseaseRepository : IRepository<ChronicDisease>
{
    Task<IEnumerable<ChronicDisease>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<ChronicDisease>> GetActivePagedAsync(PaginationRequest request, CancellationToken cancellationToken = default);
    Task<ChronicDisease> AddAsync(ChronicDisease entity, CancellationToken cancellationToken = default);
    void Update(ChronicDisease entity);
    void Delete(ChronicDisease entity);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}