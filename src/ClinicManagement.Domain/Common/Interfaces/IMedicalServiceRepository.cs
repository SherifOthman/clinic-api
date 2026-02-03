using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IMedicalServiceRepository : IRepository<MedicalService>
{
    Task<IEnumerable<MedicalService>> GetByClinicIdAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<MedicalService?> GetByIdAndClinicIdAsync(Guid id, Guid clinicId, CancellationToken cancellationToken = default);
}