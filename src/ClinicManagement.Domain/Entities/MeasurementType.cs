using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Defines the types of measurements that can be taken (e.g., Blood Pressure, Weight, Height, etc.)
/// This is the "Attribute" part of the EAV model
/// </summary>
public class MeasurementType : AuditableEntity
{
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Unit { get; set; } = null!; // e.g., "mmHg", "kg", "cm", "mg/dL"
    public MeasurementDataType DataType { get; set; } // String, Number, Boolean, Date
    public string? NormalRange { get; set; } // e.g., "120/80", "18.5-24.9", ">= 12"
    public bool IsRequired { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    
    // For structured measurements like Blood Pressure (systolic/diastolic)
    public bool HasMultipleValues { get; set; } = false;
    public string? ValueLabels { get; set; } // JSON array: ["Systolic", "Diastolic"]
    
    // Navigation properties
    public Clinic Clinic { get; set; } = null!;
    public ICollection<PatientMeasurement> PatientMeasurements { get; set; } = new List<PatientMeasurement>();
}