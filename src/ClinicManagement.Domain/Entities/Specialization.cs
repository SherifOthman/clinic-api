using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Specialization : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    public ICollection<SpecializationMeasurementAttribute> DefaultMeasurementAttributes { get; set; } = new List<SpecializationMeasurementAttribute>();
}