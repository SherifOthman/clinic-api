using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Junction table between MedicalVisit and MeasurementAttribute
/// Stores the actual measurement values for each visit
/// </summary>
public class MedicalVisitMeasurement : BaseEntity
{
    public int MedicalVisitId { get; set; }
    public int MeasurementAttributeId { get; set; }  
    
    public string? StringValue { get; set; }
    public int? IntValue { get; set; }
    public decimal? DecimalValue { get; set; }
    public DateTime? DateTimeValue { get; set; }
    public bool? BooleanValue { get; set; }
    
    public string? Notes { get; set; }
}
