using ClinicManagement.API.Common;

namespace ClinicManagement.API.Entities;

/// <summary>
/// Default measurement attributes for each specialization
/// When a new doctor registers, these measurements are automatically added to their profile
/// </summary>
public class SpecializationMeasurementAttribute : BaseEntity
{
    public Guid SpecializationId { get; set; }
    public Guid MeasurementAttributeId { get; set; }
    
    public int DefaultDisplayOrder { get; set; } = 0;

    public bool DefaultIsRequired { get; set; } = false;
    
    // Navigation properties
    public Specialization Specialization { get; set; } = null!;
    public MeasurementAttribute MeasurementAttribute { get; set; } = null!;
}
