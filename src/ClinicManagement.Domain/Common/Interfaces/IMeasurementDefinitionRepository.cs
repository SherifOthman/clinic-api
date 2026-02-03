using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IMeasurementDefinitionRepository : IRepository<MeasurementDefinition>
{
    Task<List<MeasurementDefinition>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<MeasurementDefinition?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}