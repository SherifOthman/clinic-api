using System;
using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Enums;

namespace ClinicManagement.API.Entities;

public class PatientMedicalFile : AuditableEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    
    public Guid? VisitId { get; set; }
    public MedicalVisit? Visit { get; set; }
    
    public ServiceType FileType { get; set; } // Lab / Radiology
    public string FileUrl { get; set; } = null!;
}
