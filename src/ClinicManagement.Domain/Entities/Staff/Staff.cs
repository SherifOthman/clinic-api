using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Staff : AuditableTenantEntity
{
    public Guid UserId { get; init; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public User User { get; set; } = null!;
    public Doctor? DoctorProfile { get; set; }
}
