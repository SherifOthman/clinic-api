using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IDoctorRepository : IRepository<Doctor>
{
    Task<IEnumerable<Doctor>> GetBySpecializationAsync(int specializationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Doctor>> GetActiveDoctorsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Doctor>> GetByBranchIdAsync(int branchId, CancellationToken cancellationToken = default);
    Task<Doctor?> GetWithSpecializationAsync(int doctorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Doctor>> GetDoctorsWithDetailsAsync(int? specializationId = null, CancellationToken cancellationToken = default);
}
