using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class UserClinic : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid ClinicId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPrimary { get; set; } = false;
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Clinic Clinic { get; set; } = null!;
}