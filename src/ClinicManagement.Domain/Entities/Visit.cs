using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Visit : AuditableEntity
{
    public Guid ClinicId { get; set; }
    public Guid ClinicPatientId { get; set; } // Reference to ClinicPatient instead of Patient directly
    public Guid DoctorId { get; set; }
    public DateTime VisitDate { get; set; }
    public string? Diagnosis { get; set; }
    public string? Notes { get; set; }
    public string? Prescription { get; set; }
    
    // Navigation properties
    public ClinicPatient ClinicPatient { get; set; } = null!;
    public User Doctor { get; set; } = null!;
    public Clinic Clinic { get; set; } = null!;
    
    // Related transactions and measurements
    public ICollection<PatientTransaction> Transactions { get; set; } = new List<PatientTransaction>();
    public ICollection<PatientMeasurement> Measurements { get; set; } = new List<PatientMeasurement>();
}