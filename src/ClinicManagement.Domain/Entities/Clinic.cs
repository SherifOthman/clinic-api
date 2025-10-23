using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Clinic : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int OwnerId { get; set; }
    public int SubscriptionPlanId { get; set; }
    
    // Navigation properties
    public virtual User Owner { get; set; } = null!;
    public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    public virtual ICollection<ClinicBranch> Branches { get; set; } = new List<ClinicBranch>();
    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();
    public virtual ICollection<ClinicSettings> Settings { get; set; } = new List<ClinicSettings>();
    
    // Domain methods
    public void Deactivate()
    {
        IsActive = false;
        EndDate = DateTime.UtcNow;
    }
    
    public void Activate()
    {
        IsActive = true;
        EndDate = null;
    }
    
    public bool CanAddDoctor(int currentDoctorCount)
    {
        return currentDoctorCount < SubscriptionPlan.DoctorLimit;
    }
    
    public bool CanAddAppointment(int currentAppointmentCount)
    {
        return currentAppointmentCount < SubscriptionPlan.AppointmentLimit;
    }
}
