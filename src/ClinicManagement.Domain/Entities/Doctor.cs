using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Doctor : AuditableEntity
{
    public Guid SpecializationId { get; set; }
    public short? YearsOfExperience { get; set; }

    // Navigation properties
    public virtual Specialization Specialization { get; set; } = null!;
    public ICollection<DoctorMeasurementAttribute> MeasurementAttributes { get; set; } = new List<DoctorMeasurementAttribute>();
}