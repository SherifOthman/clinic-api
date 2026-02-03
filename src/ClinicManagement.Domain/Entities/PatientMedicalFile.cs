using System;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class PatientMedicalFile : AuditableEntity
{
    public Guid ClinicPatientId { get; set; }
    public ClinicPatient ClinicPatient { get; set; } = null!;
    
    public Guid? VisitId { get; set; }
    public Visit? Visit { get; set; }
    
    public ServiceType FileType { get; set; } // Lab / Radiology
    public string FileUrl { get; set; } = null!;
}