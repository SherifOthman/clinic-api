using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Doctor-specific information linked to a User account
/// </summary>
public class Doctor : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid SpecializationId { get; set; }
    public string? LicenseNumber { get; set; }
    public short? YearsOfExperience { get; set; }
    public decimal? ConsultationFee { get; set; }
    public bool AvailableForEmergency { get; set; }
    public string? Biography { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Specialization Specialization { get; set; } = null!;
    public ICollection<DoctorMeasurementAttribute> MeasurementAttributes { get; set; } = new List<DoctorMeasurementAttribute>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<DoctorWorkingDay> WorkingDays { get; set; } = new List<DoctorWorkingDay>();
}
