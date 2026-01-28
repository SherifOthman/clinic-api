using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Clinic : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public int SubscriptionPlanId { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<ClinicBranch> ClinicBranches { get; set; } = new List<ClinicBranch>();
    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();
}