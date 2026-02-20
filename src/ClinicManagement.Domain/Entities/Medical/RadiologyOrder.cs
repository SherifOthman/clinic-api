using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class RadiologyOrder : BaseEntity
{
    public int ClinicBranchId { get; set; }
    public int PatientId { get; set; }
    public int RadiologyTestId { get; set; }
    public int? MedicalVisitId { get; set; }
    public int? OrderedByDoctorId { get; set; }
    public RadiologyStatus Status { get; set; }
    public DateTime OrderedAt { get; set; }
    public DateTime? PerformedAt { get; set; }
    public int? PerformedByUserId { get; set; }
    public DateTime? ResultsAvailableAt { get; set; }
    public int? ResultsUploadedByUserId { get; set; }
    public string? ImageFilePath { get; set; }
    public string? ReportFilePath { get; set; }
    public string? Findings { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedByDoctorId { get; set; }
    public string? DoctorNotes { get; set; }
    public string? Notes { get; set; }
}

public enum RadiologyStatus
{
    Ordered,
    InProgress,
    ResultsAvailable,
    Reviewed,
    Cancelled
}
