using System;
using System.Collections.Generic;
using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Clinic-specific snapshot of patient
/// </summary>
public class ClinicPatient : AuditableEntity
{
    public Guid ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;
    
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    
    public string FullName { get; set; } = null!;
    public string Gender { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Address { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    
    /// <summary>
    /// Short code for search
    /// </summary>
    public string MedicalFileNumber { get; set; } = null!;
    
    public ICollection<ClinicPatientPhone> PhoneNumbers { get; set; } = new List<ClinicPatientPhone>();
}