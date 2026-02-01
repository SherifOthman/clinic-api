using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IChronicDiseaseRepository
{
    Task<ChronicDisease?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChronicDisease>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ChronicDisease>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<ChronicDisease> AddAsync(ChronicDisease entity, CancellationToken cancellationToken = default);
    Task<ChronicDisease> UpdateAsync(ChronicDisease entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(ChronicDisease entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}