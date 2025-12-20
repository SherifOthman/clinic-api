using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Doctor : AuditableEntity
{
    public int UserId { get; set; }
    public int ClinicId { get; set; }
    public int SpecializationId { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Clinic Clinic { get; set; } = null!;
}
