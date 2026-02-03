namespace ClinicManagement.Application.DTOs;

public class VisitMeasurementDto
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public Guid MeasurementDefinitionId { get; set; }
    public DateTime MeasurementDate { get; set; }
    
    // Values
    public int? ValueInt { get; set; }
    public decimal? ValueDecimal { get; set; }
    public string? ValueText { get; set; }
    public bool? ValueBool { get; set; }
    public DateTime? ValueDate { get; set; }
    public string? StructuredValue { get; set; }
    
    public string? Notes { get; set; }
    public Guid MeasuredByUserId { get; set; }
    
    // Navigation properties
    public MeasurementDefinitionDto? MeasurementDefinition { get; set; }
    public string? MeasuredByUserName { get; set; }
}