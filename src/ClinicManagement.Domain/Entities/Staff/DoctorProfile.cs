using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Doctor-specific profile information.
/// Only created for staff members with the Doctor role.
/// </summary>
public class DoctorProfile : TenantEntity
{
    public Guid StaffId { get; set; }
    public Guid? SpecializationId { get; set; }
    public string? LicenseNumber { get; set; }

    public virtual Staff Staff { get; set; } = null!;
    public virtual Specialization? Specialization { get; set; }
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<DoctorWorkingDay> WorkingDays { get; set; } = new List<DoctorWorkingDay>();
    public virtual ICollection<DoctorMeasurementAttribute> MeasurementAttributes { get; set; } = new List<DoctorMeasurementAttribute>();
    public virtual ICollection<MedicalVisit> MedicalVisits { get; set; } = new List<MedicalVisit>();
}
