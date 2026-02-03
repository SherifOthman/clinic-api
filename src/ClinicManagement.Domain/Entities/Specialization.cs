using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Global specialization reference data (no ClinicId).
/// System Admin manages specializations.
/// Used for doctor profiles and measurement defaults.
/// </summary>
public class Specialization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<DoctorProfile> DoctorProfiles { get; set; } = new List<DoctorProfile>();
    public virtual ICollection<SpecialtyMeasurementDefault> MeasurementDefaults { get; set; } = new List<SpecialtyMeasurementDefault>();
}