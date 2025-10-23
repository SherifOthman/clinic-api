using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ClinicBranch : BaseEntity
{
    public int ClinicId { get; set; }
    public int? CityId { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    
    // Navigation properties
    public virtual Clinic Clinic { get; set; } = null!;
    public virtual City? CityNavigation { get; set; }
    public virtual ICollection<DoctorBranch> DoctorBranches { get; set; } = new List<DoctorBranch>();
    public virtual ICollection<Receptionist> Receptionists { get; set; } = new List<Receptionist>();
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
