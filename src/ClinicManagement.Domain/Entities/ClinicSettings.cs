using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ClinicSettings : BaseEntity
{
    public int ClinicId { get; set; }
    public decimal FasterPrice { get; set; }
    public decimal CheckUpPrice { get; set; }
    public decimal RevisitPrice { get; set; }
    public string Currency { get; set; } = "USD";
    
    // Navigation properties
    public virtual Clinic Clinic { get; set; } = null!;
}
