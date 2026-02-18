using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class MedicalFile : BaseEntity
{
    public string MedicalFileNumber { get; set; } = null!; // Human-readable: MF-2024-001
    public int? MedicalVisitId { get; set; }
    public MedicalVisit? MedicalVisit { get; set; }
    
    public int PatientId { get; set; }
    public MedicalFileType FileType { get; set; } // Lab / Radiology
    public DateTime UploadedAt { get; set; }
}
