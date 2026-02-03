using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Template/Seed data for measurements per specialty
/// Read-only table managed by admin
/// </summary>
public class SpecialtyMeasurementDefault : AuditableEntity
{
    public Guid SpecialtyId { get; set; }
    public Guid MeasurementDefinitionId { get; set; }
    public bool IsRequired { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    
    // Navigation properties
    public Specialization Specialty { get; set; } = null!;
    public MeasurementDefinition MeasurementDefinition { get; set; } = null!;
}