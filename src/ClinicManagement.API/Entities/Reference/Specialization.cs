using ClinicManagement.API.Common;

namespace ClinicManagement.API.Entities;

public class Specialization : BaseEntity
{
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    public ICollection<SpecializationMeasurementAttribute> DefaultMeasurementAttributes { get; set; } = new List<SpecializationMeasurementAttribute>();
}
