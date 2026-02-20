using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class MedicalFile : BaseEntity
{
    public string MedicalFileNumber { get; set; } = null!;
    public int? MedicalVisitId { get; set; }
    public int PatientId { get; set; }
    public MedicalFileType FileType { get; set; }
    public DateTime UploadedAt { get; set; }
}
