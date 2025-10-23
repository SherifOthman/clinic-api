using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DoctorLimit { get; set; }
    public int AppointmentLimit { get; set; }
    public int DurationDays { get; set; }
    public string? Features { get; set; }
    
    // Navigation properties
    public virtual ICollection<Clinic> Clinics { get; set; } = new List<Clinic>();
}
