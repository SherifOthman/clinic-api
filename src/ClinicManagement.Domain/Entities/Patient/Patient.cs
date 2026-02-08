using System;
using System.Collections.Generic;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class Patient : AuditableEntity
{
    public string PatientCode { get; set; } = null!;
    public Guid ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;   
    public string FullName { get; set; } = null!;
    public Gender Gender { get; set; }
    
    public int? CityGeoNameId { get; set; }
    
    public DateTime DateOfBirth { get; set; }
    
    public ICollection<PatientPhone> PhoneNumbers { get; set; } = new List<PatientPhone>();
    public ICollection<PatientChronicDisease> ChronicDiseases { get; set; } = new List<PatientChronicDisease>();
    public ICollection<MedicalFile> MedicalFiles { get; set; } = new List<MedicalFile>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
