using System;
using System.Collections.Generic;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Clinic-specific snapshot of patient
/// </summary>
public class ClinicPatient : AuditableEntity
{
    public string PatientNumber { get; set; } = null!; // Human-readable: PAT-2024-0001
    public Guid ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;
    
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    
    public string FullName { get; set; } = null!;
    public Gender Gender { get; set; }
    public string City { get; set; } = null!;
    public string? Address { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    
    /// <summary>
    /// Short code for search
    /// </summary>
    public string MedicalFileNumber { get; set; } = null!;
    
    public ICollection<ClinicPatientPhone> PhoneNumbers { get; set; } = new List<ClinicPatientPhone>();
    public ICollection<ClinicPatientChronicDisease> ChronicDiseases { get; set; } = new List<ClinicPatientChronicDisease>();
    public ICollection<MedicalFile> MedicalFiles { get; set; } = new List<MedicalFile>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
