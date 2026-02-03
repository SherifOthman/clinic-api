using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// ClinicPatient is a SNAPSHOT copy of Patient data for clinic independence.
/// Each clinic maintains its own patient records that can be edited freely.
/// Changes must NOT affect other clinics or the global Patient.
/// </summary>
public class ClinicPatient : AuditableEntity
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    
    // Clinic-specific patient data (snapshot from Patient)
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string City { get; set; } = string.Empty;
    
    // Clinic-scoped medical file number (auto-generated, human-readable)
    public string MedicalFileNumber { get; set; } = string.Empty; // e.g., "CLN-1023"
    
    // Navigation properties
    public Clinic Clinic { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    
    // Clinic-specific data
    public ICollection<ClinicPatientPhoneNumber> PhoneNumbers { get; set; } = new List<ClinicPatientPhoneNumber>();
    public ICollection<PatientChronicDisease> ChronicDiseases { get; set; } = new List<PatientChronicDisease>();
    
    // Clinical operations
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<LabResult> LabResults { get; set; } = new List<LabResult>();
    public ICollection<RadiologyResult> RadiologyResults { get; set; } = new List<RadiologyResult>();
    
    // Legacy - keeping for backward compatibility
    [Obsolete("Use Visit.ServiceItems instead")]
    public ICollection<PatientTransaction> Transactions { get; set; } = new List<PatientTransaction>();
}
