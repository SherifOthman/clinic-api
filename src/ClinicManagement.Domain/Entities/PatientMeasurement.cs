using System;
using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class PatientMeasurement : BaseEntity
{
    public Guid ClinicPatientId { get; set; }
    public ClinicPatient ClinicPatient { get; set; } = null!;
    
    public Guid MeasurementAttributeId { get; set; }
    public MeasurementAttribute MeasurementAttribute { get; set; } = null!;
    
    public string Value { get; set; } = null!;
    public DateTime RecordedAt { get; set; }
}