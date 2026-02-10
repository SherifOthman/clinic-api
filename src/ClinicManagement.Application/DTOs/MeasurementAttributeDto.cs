using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Application.DTOs;

public class MeasurementAttributeDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public MeasurementDataType DataType { get; set; }
}
