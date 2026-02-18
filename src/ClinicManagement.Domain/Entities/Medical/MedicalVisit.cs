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
    public int ClinicBranchId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int AppointmentId { get; set; }
    public string? Diagnosis { get; set; }
}
