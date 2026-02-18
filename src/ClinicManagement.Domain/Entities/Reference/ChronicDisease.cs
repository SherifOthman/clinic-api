using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ChronicDisease : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    

    public ICollection<PatientChronicDisease> Patients { get; set; } = new List<PatientChronicDisease>();
}
