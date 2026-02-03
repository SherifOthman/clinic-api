using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IChronicDiseaseRepository : IRepository<ChronicDisease>
{
    Task<IEnumerable<ChronicDisease>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<ChronicDisease>> GetActivePagedAsync(PaginationRequest request, CancellationToken cancellationToken = default);
}
