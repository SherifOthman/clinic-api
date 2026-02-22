using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Repositories;

public interface IDoctorProfileRepository : IRepository<DoctorProfile>
{
    Task<DoctorProfile?> GetByStaffIdAsync(int staffId, CancellationToken cancellationToken = default);
}
