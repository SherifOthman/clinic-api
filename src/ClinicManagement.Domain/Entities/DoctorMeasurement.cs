using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Doctor's working set of measurements
/// This is what the system actually works with
/// Copied from SpecialtyMeasurementDefaults when doctor is created
/// </summary>
public class DoctorMeasurement : AuditableEntity
{
    public Guid DoctorId { get; set; } // FK to User (doctor)
    public Guid MeasurementDefinitionId { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsRequired { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    
    // Navigation properties
    public User Doctor { get; set; } = null!;
    public MeasurementDefinition MeasurementDefinition { get; set; } = null!;
}