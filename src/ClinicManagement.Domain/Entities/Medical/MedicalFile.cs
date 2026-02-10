using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class MedicalFile : BaseEntity
{
    public string MedicalFileNumber { get; set; } = null!; // Human-readable: MF-2024-001
    public Guid? MedicalVisitId { get; set; }
    public MedicalVisit? MedicalVisit { get; set; }
    
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    
    public string FilePath { get; set; } = null!;
    public MedicalFileType FileType { get; set; } // Lab / Radiology
    public DateTime UploadedAt { get; set; }
}
