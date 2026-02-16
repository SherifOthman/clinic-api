using ClinicManagement.API.Common;

namespace ClinicManagement.API.Entities;

public class LabTestOrder : BaseEntity
{
    public Guid ClinicBranchId { get; set; }
    public Guid PatientId { get; set; }
    public Guid LabTestId { get; set; }
    public Guid? MedicalVisitId { get; set; }
    public Guid? OrderedByDoctorId { get; set; }
    public LabTestStatus Status { get; set; }
    public DateTime OrderedAt { get; set; }
    public DateTime? PerformedAt { get; set; }
    public Guid? PerformedByUserId { get; set; }
    public DateTime? ResultsAvailableAt { get; set; }
    public Guid? ResultsUploadedByUserId { get; set; }
    public string? ResultFilePath { get; set; }
    public string? ResultNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedByDoctorId { get; set; }
    public string? DoctorNotes { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public ClinicBranch ClinicBranch { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public LabTest LabTest { get; set; } = null!;
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
