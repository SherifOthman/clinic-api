using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Stores actual measurement values for patients using EAV pattern
/// This is the "Value" part of the EAV model, with ClinicPatient being the "Entity"
/// </summary>
public class PatientMeasurement : AuditableEntity
{
    public Guid ClinicPatientId { get; set; } // Reference to ClinicPatient instead of Patient directly
    public Guid MeasurementTypeId { get; set; }
    public Guid? VisitId { get; set; } // Optional - can be linked to a specific visit
    public DateTime MeasurementDate { get; set; }
    
    // Value storage - only one of these should be populated based on MeasurementType.DataType
    public string? StringValue { get; set; }
    public decimal? NumericValue { get; set; }
    public bool? BooleanValue { get; set; }
    public DateTime? DateValue { get; set; }
    
    // For complex measurements like Blood Pressure (120/80)
    public string? StructuredValue { get; set; } // JSON format: {"systolic": 120, "diastolic": 80}
    
    public string? Notes { get; set; }
    public Guid MeasuredByUserId { get; set; }
    
    // Navigation properties
    public ClinicPatient ClinicPatient { get; set; } = null!;
    public MeasurementType MeasurementType { get; set; } = null!;
    public Visit? Visit { get; set; }
    public User MeasuredByUser { get; set; } = null!;
}