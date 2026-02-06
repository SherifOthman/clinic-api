using System;
using System.Collections.Generic;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Medical visit, linked optionally to Appointment
/// </summary>
public class MedicalVisit : BaseEntity
{
    public Guid ClinicBranchId { get; set; }
    public ClinicBranch ClinicBranch { get; set; } = null!;
    
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;

    public string? Diagnosis { get; set; }
    
    public Prescription? Prescription { get; set; }
    
    // Navigation properties for lab tests, radiology tests, medical files, and measurements
    public ICollection<MedicalVisitLabTest> LabTests { get; set; } = new List<MedicalVisitLabTest>();
    public ICollection<MedicalVisitRadiology> RadiologyTests { get; set; } = new List<MedicalVisitRadiology>();
    public ICollection<MedicalFile> MedicalFiles { get; set; } = new List<MedicalFile>();
    public ICollection<MedicalVisitMeasurement> Measurements { get; set; } = new List<MedicalVisitMeasurement>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
