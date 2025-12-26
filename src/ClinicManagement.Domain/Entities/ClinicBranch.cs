using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ClinicBranch : AuditableEntity
{
    public int ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual Clinic Clinic { get; set; } = null!;
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}