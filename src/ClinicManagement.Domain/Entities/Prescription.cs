using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Prescription is linked to Visit.
/// Contains medications, lab requests, radiology requests.
/// Each item may contain instructions.
/// </summary>
public class Prescription : AuditableEntity
{
    public Guid VisitId { get; set; }
    public DateTime PrescriptionDate { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public Visit Visit { get; set; } = null!;
    public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
}