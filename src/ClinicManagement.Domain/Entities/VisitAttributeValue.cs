using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class VisitAttributeValue : BaseEntity
{
    public int VisitId { get; set; }
    public int FieldId { get; set; }
    public string? Value { get; set; }
    
    // Navigation properties
    public virtual Visit Visit { get; set; } = null!;
    public virtual VisitAttributes Field { get; set; } = null!;
}
