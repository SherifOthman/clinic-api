using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IDoctorProfileRepository : IRepository<Doctor>
{
    /// <summary>Get the DoctorProfile ID for a given staff member.</summary>
    Task<Guid> GetIdByStaffIdAsync(Guid staffId, CancellationToken ct = default);

    /// <summary>Get the full Doctor entity by its own ID.</summary>
    new Task<Doctor?> GetByIdAsync(Guid doctorId, CancellationToken ct = default);
}
