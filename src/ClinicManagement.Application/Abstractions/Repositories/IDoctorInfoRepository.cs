using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

/// <summary>Replaces IDoctorProfileRepository — works with DoctorInfo instead of Doctor.</summary>
public interface IDoctorInfoRepository : IRepository<DoctorInfo>
{
    /// <summary>Get the DoctorInfo ID for a given ClinicMember.</summary>
    Task<Guid> GetIdByMemberIdAsync(Guid clinicMemberId, CancellationToken ct = default);

    /// <summary>Get the full DoctorInfo entity by its own ID.</summary>
    new Task<DoctorInfo?> GetByIdAsync(Guid doctorInfoId, CancellationToken ct = default);
}
