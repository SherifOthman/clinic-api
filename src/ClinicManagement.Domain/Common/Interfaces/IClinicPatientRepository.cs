using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IClinicPatientRepository : IRepository<ClinicPatient>
{
    Task<IEnumerable<ClinicPatient>> GetByClinicIdAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<PagedResult<ClinicPatient>> GetByClinicIdPagedAsync(Guid clinicId, SearchablePaginationRequest request, CancellationToken cancellationToken = default);
    Task<ClinicPatient?> GetByIdAndClinicIdAsync(Guid id, Guid clinicId, CancellationToken cancellationToken = default);
    Task<ClinicPatient?> GetByPatientIdAndClinicIdAsync(Guid patientId, Guid clinicId, CancellationToken cancellationToken = default);
}
