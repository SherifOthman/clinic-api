using System;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class PatientMedicalFile : AuditableEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    
    public Guid? VisitId { get; set; }
    public MedicalVisit? Visit { get; set; }
    
    public ServiceType FileType { get; set; } // Lab / Radiology
    public string FileUrl { get; set; } = null!;
}
