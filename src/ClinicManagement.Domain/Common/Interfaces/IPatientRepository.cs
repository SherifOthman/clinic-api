using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IPatientRepository : IRepository<Patient>
{
    Task<IEnumerable<Patient>> GetByClinicIdAsync(int clinicId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Patient>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<Patient>> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<Patient?> GetWithAppointmentsAsync(int patientId, CancellationToken cancellationToken = default);
}
