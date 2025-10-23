using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Doctor : BaseEntity
{
    public int UserId { get; set; }
    public int SpecializationId { get; set; }
    public string? Bio { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Specialization Specialization { get; set; } = null!;
    public virtual ICollection<DoctorBranch> DoctorBranches { get; set; } = new List<DoctorBranch>();
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
