using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class UserClinic : BaseEntity
{
    public int UserId { get; set; }
    public int ClinicId { get; set; }
    public bool IsOwner { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime JoinedAt { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Clinic Clinic { get; set; } = null!;
}