using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Doctor's working set of measurements.
/// Copied from SpecialtyMeasurementDefault when doctor is created.
/// Doctor can add/remove/edit measurements independently.
/// </summary>
public class DoctorMeasurement : AuditableEntity
{
    public Guid StaffId { get; set; } // FK to Staff (must be doctor)
    public Guid MeasurementDefinitionId { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsRequired { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    
    // Navigation properties
    public Staff Staff { get; set; } = null!;
    public MeasurementDefinition MeasurementDefinition { get; set; } = null!;
}