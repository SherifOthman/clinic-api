using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class PatientChronicDisease : BaseEntity
{
    public Guid PatientId { get; set; }
    public Guid ChronicDiseaseId { get; set; }
    public DateTime DiagnosedDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual Patient Patient { get; set; } = null!;
    public virtual ChronicDisease ChronicDisease { get; set; } = null!;
}