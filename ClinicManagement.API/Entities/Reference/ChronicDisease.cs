using ClinicManagement.API.Common;

namespace ClinicManagement.API.Entities;

public class ChronicDisease : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    
    // Navigation properties
    public ICollection<PatientChronicDisease> Patients { get; set; } = new List<PatientChronicDisease>();
}
