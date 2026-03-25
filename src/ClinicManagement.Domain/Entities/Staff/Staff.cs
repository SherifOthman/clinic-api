using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Staff : TenantEntity
{
    public Guid UserId { get; init; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Clinic Clinic { get; set; } = null!;
    public DoctorProfile? DoctorProfile { get; set; }
}
