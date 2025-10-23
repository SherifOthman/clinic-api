using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class VisitAttributes : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual ICollection<SpecializationAttribute> SpecializationAttributes { get; set; } = new List<SpecializationAttribute>();
    public virtual ICollection<VisitAttributeValue> VisitAttributeValues { get; set; } = new List<VisitAttributeValue>();
}
