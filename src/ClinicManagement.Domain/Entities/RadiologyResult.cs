using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Radiology results belong to ClinicPatient.
/// May optionally link to Visit (if requested during visit).
/// Can be uploaded without a visit (old files).
/// </summary>
public class RadiologyResult : AuditableEntity
{
    public Guid ClinicPatientId { get; set; }
    public Guid? VisitId { get; set; } // Optional - may be uploaded without visit
    
    public string StudyName { get; set; } = string.Empty;
    public string? StudyCode { get; set; }
    public DateTime StudyDate { get; set; }
    public DateTime? ReportDate { get; set; }
    
    // Result data
    public string? Findings { get; set; }
    public string? Impression { get; set; }
    public string? Recommendation { get; set; }
    public string? Notes { get; set; }
    
    // File attachments
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    
    // Radiology information
    public string? RadiologyCenter { get; set; }
    public string? RadiologistName { get; set; }
    public bool IsInternal { get; set; } = false; // Internal (clinic performs) vs External (file upload)
    
    // Navigation properties
    public ClinicPatient ClinicPatient { get; set; } = null!;
    public Visit? Visit { get; set; }
}