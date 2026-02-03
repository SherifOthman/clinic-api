using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Application.DTOs;

public class MeasurementDefinitionDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public MeasurementDataType DataType { get; set; }
    public string Unit { get; set; } = null!;
    public string? NormalRange { get; set; }
    public bool IsActive { get; set; }
    public bool HasMultipleValues { get; set; }
    public string? ValueLabels { get; set; }
}