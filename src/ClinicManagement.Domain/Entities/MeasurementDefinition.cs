using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Defines measurement types (e.g., Blood Pressure, Weight, Height, etc.)
/// This is the master definition table
/// </summary>
public class MeasurementDefinition : AuditableEntity
{
    public string Code { get; set; } = null!; // Unique code like "BP", "WEIGHT", "HEIGHT"
    public string Name { get; set; } = null!; // Display name
    public string? Description { get; set; }
    public MeasurementDataType DataType { get; set; } // String, Number, Boolean, Date, Structured
    public string Unit { get; set; } = null!; // e.g., "mmHg", "kg", "cm", "mg/dL"
    public string? NormalRange { get; set; } // e.g., "120/80", "18.5-24.9", ">= 12"
    public bool IsActive { get; set; } = true;
    
    // For structured measurements like Blood Pressure (systolic/diastolic)
    public bool HasMultipleValues { get; set; } = false;
    public string? ValueLabels { get; set; } // JSON array: ["Systolic", "Diastolic"]
    
    // Navigation properties
    public ICollection<SpecialtyMeasurementDefault> SpecialtyDefaults { get; set; } = new List<SpecialtyMeasurementDefault>();
    public ICollection<DoctorMeasurement> DoctorMeasurements { get; set; } = new List<DoctorMeasurement>();
    public ICollection<VisitMeasurement> VisitMeasurements { get; set; } = new List<VisitMeasurement>();
}