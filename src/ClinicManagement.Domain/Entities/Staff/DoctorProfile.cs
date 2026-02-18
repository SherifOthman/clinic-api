using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Doctor-specific profile information.
/// Only created for staff members with the Doctor role.
/// </summary>
public class DoctorProfile : TenantEntity
{
    public int StaffId { get; set; }
    public int? SpecializationId { get; set; }
    public string? LicenseNumber { get; set; }
}
