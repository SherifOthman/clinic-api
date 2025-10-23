using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class PatientSurgery : BaseEntity
{
    public int PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Navigation properties
    public virtual Patient Patient { get; set; } = null!;
}
