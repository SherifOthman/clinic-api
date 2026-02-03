using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IDoctorRepository : IRepository<Doctor>
{
    Task<IEnumerable<Doctor>> GetDoctorsBySpecializationAsync(Guid specializationId, CancellationToken cancellationToken = default);
    Task<Doctor?> GetDoctorWithSpecializationAsync(Guid doctorId, CancellationToken cancellationToken = default);
}