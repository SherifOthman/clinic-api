using ClinicManagement.API.Common;

namespace ClinicManagement.API.Entities;

/// <summary>
/// Doctor-specific profile information.
/// Only created for staff members with the Doctor role.
/// </summary>
public class DoctorProfile : TenantEntity
{
    /// <summary>
    /// Reference to the Staff record
    /// </summary>
    public Guid StaffId { get; set; }
    
    /// <summary>
    /// Doctor's medical specialization
    /// </summary>
    public Guid? SpecializationId { get; set; }
    
    /// <summary>
    /// Medical license number
    /// </summary>
    public string? LicenseNumber { get; set; }
    
    // Navigation properties
    public virtual Staff Staff { get; set; } = null!;
    public virtual Specialization? Specialization { get; set; }
    
    // Appointments are linked through DoctorProfile
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<DoctorWorkingDay> WorkingDays { get; set; } = new List<DoctorWorkingDay>();
    public virtual ICollection<DoctorMeasurementAttribute> MeasurementAttributes { get; set; } = new List<DoctorMeasurementAttribute>();
    public virtual ICollection<MedicalVisit> MedicalVisits { get; set; } = new List<MedicalVisit>();
}
