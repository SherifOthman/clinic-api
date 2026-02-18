using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Junction table between Doctor and MeasurementAttribute
/// Defines which measurements each doctor wants to use during visits
/// </summary>
public class DoctorMeasurementAttribute : BaseEntity
{
    public int DoctorId { get; set; }
    public int MeasurementAttributeId { get; set; }
    
    public int DisplayOrder { get; set; } = 0;

    public bool IsRequired { get; set; } = false;
}
