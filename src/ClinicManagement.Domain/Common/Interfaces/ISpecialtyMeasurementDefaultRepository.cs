using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface ISpecialtyMeasurementDefaultRepository : IRepository<SpecialtyMeasurementDefault>
{
    Task<List<SpecialtyMeasurementDefault>> GetBySpecialtyIdAsync(Guid specialtyId, CancellationToken cancellationToken = default);
}