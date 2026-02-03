namespace ClinicManagement.Application.DTOs;

public class DoctorMeasurementDto
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public Guid MeasurementDefinitionId { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    
    // Navigation properties
    public MeasurementDefinitionDto? MeasurementDefinition { get; set; }
}