using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ChronicDisease: BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    
    // Navigation properties
    public virtual ICollection<ClinicPatient> PatientChronicDiseases { get; set; } = new List<ClinicPatient>();
}