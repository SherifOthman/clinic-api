using System;
using System.Collections.Generic;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Medical visit, linked optionally to Appointment
/// </summary>
public class Visit : AuditableEntity
{
    public Guid ClinicBranchId { get; set; }
    public ClinicBranch ClinicBranch { get; set; } = null!;
    
    public Guid ClinicPatientId { get; set; }
    public ClinicPatient ClinicPatient { get; set; } = null!;
    
    public Guid DoctorId { get; set; }
    public Staff Doctor { get; set; } = null!;
    
    public Guid? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }
    
    public VisitType VisitType { get; set; }
    public string? Diagnosis { get; set; }
    
    public ICollection<VisitServiceItem> ServiceItems { get; set; } = new List<VisitServiceItem>();
    public Prescription? Prescription { get; set; }
}