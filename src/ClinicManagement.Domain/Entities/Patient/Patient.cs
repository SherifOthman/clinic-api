using System;
using System.Collections.Generic;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Clinic-specific snapshot of patient
/// </summary>
public class Patient : AuditableEntity
{
    public string PatientCode { get; set; } = null!; // Human-readable: PAT-2024-0001
    public Guid ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;   
    public string FirstName { get; set; } = null!;
    public string MiddleName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public Gender Gender { get; set; }
    public string City { get; set; } = null!;
    public string? Address { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    
    public ICollection<PatientPhone> PhoneNumbers { get; set; } = new List<PatientPhone>();
    public ICollection<PatientChronicDisease> ChronicDiseases { get; set; } = new List<PatientChronicDisease>();
    public ICollection<MedicalFile> MedicalFiles { get; set; } = new List<MedicalFile>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
