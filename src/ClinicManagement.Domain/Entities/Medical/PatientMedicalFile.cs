using System;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class PatientMedicalFile : AuditableEntity
{
    public int PatientId { get; set; }
    public int? VisitId { get; set; }
    public ServiceType FileType { get; set; }
}
