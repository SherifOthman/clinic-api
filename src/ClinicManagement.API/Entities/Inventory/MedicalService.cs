using ClinicManagement.API.Common;

namespace ClinicManagement.API.Entities;

public class MedicalService : AuditableEntity
{
    public Guid ClinicBranchId { get; set; } // Linked to branch, not clinic
    public string Name { get; set; } = null!;
    public decimal DefaultPrice { get; set; }
    public bool IsOperation { get; set; } // Is it a surgical operation?

    // Navigation properties
    public ClinicBranch ClinicBranch { get; set; } = null!;
    public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}
