using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Visit is the core medical entity.
/// Can exist WITH or WITHOUT appointment.
/// Holds diagnosis, services, prescription, labs, radiology.
/// Everything medical happens in Visit, NOT Appointment.
/// </summary>
public class Visit : AuditableEntity
{
    public Guid ClinicId { get; set; }
    public Guid ClinicPatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid? AppointmentId { get; set; } // Optional - visit may exist without appointment
    
    public DateTime VisitDate { get; set; }
    public VisitType VisitType { get; set; } // InitialVisit, FollowUp, Emergency, TreatmentSession
    public string? ChiefComplaint { get; set; }
    public string? Diagnosis { get; set; }
    public string? Notes { get; set; }
    public string? TreatmentPlan { get; set; }
    
    // Navigation properties
    public ClinicPatient ClinicPatient { get; set; } = null!;
    public User Doctor { get; set; } = null!;
    public Clinic Clinic { get; set; } = null!;
    public Appointment? Appointment { get; set; }
    
    // Medical content
    public ICollection<VisitServiceItem> ServiceItems { get; set; } = new List<VisitServiceItem>();
    public ICollection<VisitMeasurement> Measurements { get; set; } = new List<VisitMeasurement>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}