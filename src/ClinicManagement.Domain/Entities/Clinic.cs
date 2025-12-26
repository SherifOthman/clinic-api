using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Clinic : AuditableEntity
{
    public int OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public string? Website { get; set; }
    public bool IsActive { get; set; } = true;
    public bool OnboardingCompleted { get; set; } = false;
    public string? OnboardingStep { get; set; }
    public int? SubscriptionPlanId { get; set; }
    
    // Navigation properties
    public virtual User Owner { get; set; } = null!;
    public virtual ICollection<ClinicBranch> Branches { get; set; } = new List<ClinicBranch>();
    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    public virtual ICollection<Receptionist> Receptionists { get; set; } = new List<Receptionist>();
    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();
}
