using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Medicine : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Dosage { get; set; }
    public string? Form { get; set; }
    public string? Description { get; set; }
    
    // Navigation properties
    public virtual ICollection<PrescriptionMedicine> PrescriptionMedicines { get; set; } = new List<PrescriptionMedicine>();
}
