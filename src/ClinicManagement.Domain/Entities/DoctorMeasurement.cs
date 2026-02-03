using System;
using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class DoctorMeasurement : BaseEntity
{
    public Guid DoctorId { get; set; }
    public Staff Doctor { get; set; } = null!;
    
    public Guid MeasurementAttributeId { get; set; }
    public MeasurementAttribute MeasurementAttribute { get; set; } = null!;
}