using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class LabTestOrder : BaseEntity
{
    public int ClinicBranchId { get; set; }
    public int PatientId { get; set; }
    public int LabTestId { get; set; }
    public int? MedicalVisitId { get; set; }
    public int? OrderedByDoctorId { get; set; }
    public LabTestStatus Status { get; set; }
    public DateTime OrderedAt { get; set; }
    public DateTime? PerformedAt { get; set; }
    public int? PerformedByUserId { get; set; }
    public DateTime? ResultsAvailableAt { get; set; }
    public int? ResultsUploadedByUserId { get; set; }
    public string? ResultFilePath { get; set; }
    public string? ResultNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedByDoctorId { get; set; }
    public string? DoctorNotes { get; set; }
    public string? Notes { get; set; }
    public MedicalVisit? MedicalVisit { get; set; }
    public DoctorProfile? OrderedByDoctor { get; set; }
}

public enum LabTestStatus
{
    Ordered,
    InProgress,
    ResultsAvailable,
    Reviewed,
    Cancelled
}
