using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// EAV model for doctor measurements
/// Each doctor can choose which measurements are available for their visits
/// </summary>
public class MeasurementAttribute : BaseEntity
{
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public MeasurementDataType DataType { get; set; }
}
