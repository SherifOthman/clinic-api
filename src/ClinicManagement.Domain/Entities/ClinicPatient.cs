using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class ClinicPatient : AuditableEntity
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    
    // Patient data specific to this clinic
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string City { get; set; } = string.Empty;
    
    // Navigation properties
    public Clinic Clinic { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    
    // Related data
    public ICollection<PatientPhone> PhoneNumbers { get; set; } = new List<PatientPhone>();
    public ICollection<PatientChronicDisease> ChronicDiseases { get; set; } = new List<PatientChronicDisease>();
    
    // Clinical operations
    public ICollection<PatientTransaction> Transactions { get; set; } = new List<PatientTransaction>();
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
