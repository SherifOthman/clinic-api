namespace ClinicManagement.Application.DTOs;

public class MedicalVisitMeasurementDto
{
    public Guid Id { get; set; }
    public Guid MedicalVisitId { get; set; }
    public Guid MeasurementAttributeId { get; set; }
    
    public string? StringValue { get; set; }
    public int? IntValue { get; set; }
    public decimal? DecimalValue { get; set; }
    public DateTime? DateTimeValue { get; set; }
    public bool? BooleanValue { get; set; }
    
    public string? Notes { get; set; }
    
    // For display purposes
    public string MeasurementName { get; set; } = null!;
    public object? Value { get; set; }
}
