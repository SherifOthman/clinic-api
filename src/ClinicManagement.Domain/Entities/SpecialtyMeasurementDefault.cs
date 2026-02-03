using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Global template data for measurements per specialty.
/// No ClinicId - this is admin-managed reference data.
/// System Admin defines default measurements per specialty.
/// When doctor is created, these are copied to DoctorMeasurement.
/// </summary>
public class SpecialtyMeasurementDefault : AuditableEntity
{
    public Guid SpecializationId { get; set; }
    public Guid MeasurementDefinitionId { get; set; }
    public bool IsRequired { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    
    // Navigation properties
    public Specialization Specialization { get; set; } = null!;
    public MeasurementDefinition MeasurementDefinition { get; set; } = null!;
}