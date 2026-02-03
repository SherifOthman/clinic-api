using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Lab results belong to ClinicPatient.
/// May optionally link to Visit (if requested during visit).
/// Can be uploaded without a visit (old files).
/// </summary>
public class LabResult : AuditableEntity
{
    public Guid ClinicPatientId { get; set; }
    public Guid? VisitId { get; set; } // Optional - may be uploaded without visit
    
    public string TestName { get; set; } = string.Empty;
    public string? TestCode { get; set; }
    public DateTime TestDate { get; set; }
    public DateTime? ResultDate { get; set; }
    
    // Result data
    public string? ResultValue { get; set; }
    public string? ReferenceRange { get; set; }
    public string? Unit { get; set; }
    public string? Status { get; set; } // Normal, Abnormal, Critical
    public string? Notes { get; set; }
    
    // File attachments
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    
    // Lab information
    public string? LabName { get; set; }
    public bool IsInternal { get; set; } = false; // Internal (clinic performs) vs External (file upload)
    
    // Navigation properties
    public ClinicPatient ClinicPatient { get; set; } = null!;
    public Visit? Visit { get; set; }
}