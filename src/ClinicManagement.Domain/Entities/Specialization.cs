using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Specialization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    
    public virtual ICollection<DoctorProfile> Users { get; set; } = new List<DoctorProfile>();
    public virtual ICollection<SpecialtyMeasurementDefault> MeasurementDefaults { get; set; } = new List<SpecialtyMeasurementDefault>();
}