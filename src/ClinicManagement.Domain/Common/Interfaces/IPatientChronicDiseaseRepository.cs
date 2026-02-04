using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IPatientChronicDiseaseRepository : IRepository<PatientChronicDisease>
{
    Task<IEnumerable<PatientChronicDisease>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PatientChronicDisease>> GetActiveByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<PatientChronicDisease?> GetByPatientAndDiseaseAsync(Guid patientId, Guid chronicDiseaseId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid patientId, Guid chronicDiseaseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PatientChronicDisease>> GetByChronicDiseaseIdAsync(Guid chronicDiseaseId, CancellationToken cancellationToken = default);
}
