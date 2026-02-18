using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Default measurement attributes for each specialization
/// When a new doctor registers, these measurements are automatically added to their profile
/// </summary>
public class SpecializationMeasurementAttribute : BaseEntity
{
    public int SpecializationId { get; set; }
    public int MeasurementAttributeId { get; set; }
    
    public int DefaultDisplayOrder { get; set; } = 0;

    public bool DefaultIsRequired { get; set; } = false;
}
