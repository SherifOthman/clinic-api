using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Application.DTOs;

public class MeasurementAttributeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public MeasurementDataType DataType { get; set; }
}