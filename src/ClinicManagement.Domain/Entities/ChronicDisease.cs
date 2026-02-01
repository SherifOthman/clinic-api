using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ChronicDisease : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<PatientChronicDisease> PatientChronicDiseases { get; set; } = new List<PatientChronicDisease>();
}