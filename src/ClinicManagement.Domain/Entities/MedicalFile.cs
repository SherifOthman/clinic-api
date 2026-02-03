using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class MedicalFile : BaseEntity
{
    public Guid? MedicalVisitId { get; set; }
    public MedicalVisit? MedicalVisit { get; set; }
    
    public Guid ClinicPatientId { get; set; }
    public ClinicPatient ClinicPatient { get; set; } = null!;
    
    public string FilePath { get; set; } = null!;
    public MedicalFileType FileType { get; set; } // Lab / Radiology
    public DateTime UploadedAt { get; set; }
}