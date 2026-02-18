using System;
using System.Collections.Generic;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Medical visit, linked optionally to Appointment
/// </summary>
public class MedicalVisit : BaseEntity
{
    public Guid ClinicBranchId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid AppointmentId { get; set; }
    public string? Diagnosis { get; set; }
}
