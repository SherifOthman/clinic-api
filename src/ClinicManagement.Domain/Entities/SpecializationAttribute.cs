using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class SpecializationAttribute : BaseEntity
{
    public int SpecializationId { get; set; }
    public int VisitAttributeId { get; set; }
    
    // Navigation properties
    public virtual Specialization Specialization { get; set; } = null!;
    public virtual VisitAttributes VisitAttribute { get; set; } = null!;
}
