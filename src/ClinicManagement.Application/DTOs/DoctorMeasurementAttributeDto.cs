namespace ClinicManagement.Application.DTOs;

public class DoctorMeasurementAttributeDto
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public Guid MeasurementAttributeId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsRequired { get; set; }
    public bool IsActive { get; set; }
    
    // For display purposes
    public string MeasurementName { get; set; } = null!;
    public string DataType { get; set; } = null!;
}
