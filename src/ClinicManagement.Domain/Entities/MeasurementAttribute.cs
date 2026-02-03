using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// EAV model for doctor measurements
/// Each doctor can choose which measurements are available for their visits
/// </summary>
public class MeasurementAttribute : BaseEntity
{
    public string Name { get; set; } = null!;
    public MeasurementDataType DataType { get; set; }
    
    /// <summary>
    /// Default flag for system-defined measurements per specialty
    /// </summary>
    public bool IsDefault { get; set; } = false;
}