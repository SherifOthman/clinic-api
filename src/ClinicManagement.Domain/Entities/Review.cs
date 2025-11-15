using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Review : AuditableEntity
{
    public int UserId { get; set; }
    public int ClinicId { get; set; }
    public string Quote { get; set; } = string.Empty;
    public int Rating { get; set; } = 5;
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Clinic Clinic { get; set; } = null!;
}
