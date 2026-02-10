using System;
using System.Collections.Generic;
using System.Linq;
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
    
    // Calculated properties - pure business logic
    public int Age => DateTime.UtcNow.Year - DateOfBirth.Year - 
        (DateTime.UtcNow.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
    
    public bool IsAdult => Age >= 18;
    public bool IsMinor => Age < 18;
    public bool IsSenior => Age >= 65;
    
    public string PrimaryPhoneNumber => PhoneNumbers.FirstOrDefault()?.PhoneNumber ?? string.Empty;
    
    public bool HasChronicDiseases => ChronicDiseases.Any();
    public int ChronicDiseaseCount => ChronicDiseases.Count;
}
