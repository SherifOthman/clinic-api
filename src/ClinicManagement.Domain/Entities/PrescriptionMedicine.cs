using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class PrescriptionMedicine : BaseEntity
{
    public int VisitId { get; set; }
    public int MedicineId { get; set; }
    public string? DosageInstructions { get; set; }
    public string? Duration { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual Visit Visit { get; set; } = null!;
    public virtual Medicine Medicine { get; set; } = null!;
}
