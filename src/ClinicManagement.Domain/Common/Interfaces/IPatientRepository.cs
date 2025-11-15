using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IPatientRepository : IRepository<Patient>
{
    Task<IEnumerable<Patient>> GetByClinicIdAsync(int clinicId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Patient>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<Patient>> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<Patient?> GetWithSurgeriesAsync(int patientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Patient>> GetPatientsPagedAsync(int? clinicId, string? searchTerm, Gender? gender, string? city, int? minAge, int? maxAge, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
