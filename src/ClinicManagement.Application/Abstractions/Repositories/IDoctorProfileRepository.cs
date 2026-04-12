using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IDoctorProfileRepository : IRepository<DoctorProfile>
{
    /// <summary>Get the DoctorProfile ID for a given staff member.</summary>
    Task<Guid> GetIdByStaffIdAsync(Guid staffId, CancellationToken ct = default);
}
