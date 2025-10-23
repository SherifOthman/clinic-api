using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Specialization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Navigation properties
    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    public virtual ICollection<SpecializationAttribute> SpecializationAttributes { get; set; } = new List<SpecializationAttribute>();
}
