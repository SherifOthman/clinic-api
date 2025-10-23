using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Country : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? FlagUrl { get; set; }
    
    // Navigation properties
    public virtual ICollection<Governorate> Governorates { get; set; } = new List<Governorate>();
}
