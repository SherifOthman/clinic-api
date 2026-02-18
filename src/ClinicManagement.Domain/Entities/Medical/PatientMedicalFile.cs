using System;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class PatientMedicalFile : AuditableEntity
{
    public Guid PatientId { get; set; }
    
    public Guid? VisitId { get; set; }
    public MedicalVisit? Visit { get; set; }
    
    public ServiceType FileType { get; set; } // Lab / Radiology
}
