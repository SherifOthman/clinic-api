using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Repositories;

public interface IDoctorSpecializationRepository : IRepository<DoctorSpecialization>
{
    Task<IEnumerable<DoctorSpecialization>> GetByDoctorIdAsync(Guid doctorProfileId, CancellationToken cancellationToken = default);
    Task UpdatePrimaryAsync(Guid doctorProfileId, Guid specializationId, CancellationToken cancellationToken = default);
}
