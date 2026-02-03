using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IClinicPatientChronicDiseaseRepository : IRepository<ClinicPatientChronicDisease>
{
    Task<IEnumerable<ClinicPatientChronicDisease>> GetByClinicPatientIdAsync(Guid clinicPatientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClinicPatientChronicDisease>> GetActiveByClinicPatientIdAsync(Guid clinicPatientId, CancellationToken cancellationToken = default);
    Task<ClinicPatientChronicDisease?> GetByClinicPatientAndDiseaseAsync(Guid clinicPatientId, Guid chronicDiseaseId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid clinicPatientId, Guid chronicDiseaseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClinicPatientChronicDisease>> GetByChronicDiseaseIdAsync(Guid chronicDiseaseId, CancellationToken cancellationToken = default);
}