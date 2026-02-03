using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Clinic : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid SubscriptionPlanId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public SubscriptionPlan? SubscriptionPlan { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<ClinicPatient> Patients { get; set; } = new List<ClinicPatient>();
    public ICollection<MedicalService> Services { get; set; } = new List<MedicalService>();
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    public ICollection<MeasurementType> MeasurementTypes { get; set; } = new List<MeasurementType>();
    public ICollection<PatientTransaction> Transactions { get; set; } = new List<PatientTransaction>();
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
}