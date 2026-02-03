using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Clinic is the tenant root entity.
/// Each clinic is a separate tenant with isolated data.
/// Clinics may have multiple branches.
/// </summary>
public class Clinic : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid SubscriptionPlanId { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Clinic settings
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }

    // Navigation properties
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    public ICollection<ClinicBranch> Branches { get; set; } = new List<ClinicBranch>();
    public ICollection<Staff> Staff { get; set; } = new List<Staff>();
    public ICollection<ClinicPatient> Patients { get; set; } = new List<ClinicPatient>();
    public ICollection<MedicalService> Services { get; set; } = new List<MedicalService>();
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    public ICollection<ClinicMedication> Medications { get; set; } = new List<ClinicMedication>();
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    public ICollection<PatientTransaction> Transactions { get; set; } = new List<PatientTransaction>();
}