using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// EAV model for doctor measurements
/// Each doctor can choose which measurements are available for their visits
/// </summary>
public class MeasurementAttribute : BaseEntity
{
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public MeasurementDataType DataType { get; set; }
    

    public ICollection<MedicalVisitMeasurement> VisitMeasurements { get; set; } = new List<MedicalVisitMeasurement>();
    public ICollection<DoctorMeasurementAttribute> DoctorMeasurements { get; set; } = new List<DoctorMeasurementAttribute>();
    public ICollection<SpecializationMeasurementAttribute> SpecializationDefaults { get; set; } = new List<SpecializationMeasurementAttribute>();
}
