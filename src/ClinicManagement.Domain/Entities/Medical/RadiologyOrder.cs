using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class RadiologyOrder : BaseEntity
{
    public Guid ClinicBranchId { get; set; }
    public Guid PatientId { get; set; }
    public Guid RadiologyTestId { get; set; }
    public Guid? MedicalVisitId { get; set; }
    public Guid? OrderedByDoctorId { get; set; }
    public RadiologyStatus Status { get; set; }
    public DateTimeOffset OrderedAt { get; set; }
    public DateTimeOffset? PerformedAt { get; set; }
    public Guid? PerformedByUserId { get; set; }
    public DateTimeOffset? ResultsAvailableAt { get; set; }
    public Guid? ResultsUploadedByUserId { get; set; }
    public string? ImageFilePath { get; set; }
    public string? ReportFilePath { get; set; }
    public string? Findings { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public Guid? ReviewedByDoctorId { get; set; }
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
