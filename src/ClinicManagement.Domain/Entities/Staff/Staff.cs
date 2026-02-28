using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Staff : TenantEntity
{
    public Guid UserId { get; init; }
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; private set; }
    
    public bool IsPrimaryClinic { get; set; } = false;
    public StaffStatus Status { get; set; } = StaffStatus.Active;
    public DateTime? StatusChangedAt { get; set; }
    public Guid? StatusChangedBy { get; set; }
    public string? StatusReason { get; set; }
    public DateTime? TerminationDate { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Clinic Clinic { get; set; } = null!;
    public DoctorProfile? DoctorProfile { get; set; }
    public ICollection<StaffBranch> StaffBranches { get; set; } = new List<StaffBranch>();
}
