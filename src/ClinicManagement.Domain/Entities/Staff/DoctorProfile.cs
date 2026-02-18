using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Doctor-specific profile information.
/// Only created for staff members with the Doctor role.
/// </summary>
public class DoctorProfile : TenantEntity
{
    public Guid StaffId { get; set; }
    public Guid? SpecializationId { get; set; }
    public string? LicenseNumber { get; set; }
}
