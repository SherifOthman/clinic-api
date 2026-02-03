using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class VisitMeasurement : AuditableEntity
{
    public Guid VisitId { get; set; }
    public Guid MeasurementDefinitionId { get; set; }
    public DateTime MeasurementDate { get; set; }
    
    // Value storage - only one of these should be populated based on MeasurementDefinition.DataType
    public int? ValueInt { get; set; }
    public decimal? ValueDecimal { get; set; }
    public string? ValueText { get; set; }
    public bool? ValueBool { get; set; }
    public DateTime? ValueDate { get; set; }
    
    // For complex measurements like Blood Pressure (120/80)
    public string? StructuredValue { get; set; } // JSON format: {"systolic": 120, "diastolic": 80}
    
    public string? Notes { get; set; }
    public Guid MeasuredByUserId { get; set; }
    
    // Navigation properties
    public Visit Visit { get; set; } = null!;
    public MeasurementDefinition MeasurementDefinition { get; set; } = null!;
    public User MeasuredByUser { get; set; } = null!;
}