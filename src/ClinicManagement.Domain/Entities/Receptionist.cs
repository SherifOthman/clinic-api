using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Receptionist : BaseEntity
{
    public int UserId { get; set; }
    public int BranchId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ClinicBranch Branch { get; set; } = null!;
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
