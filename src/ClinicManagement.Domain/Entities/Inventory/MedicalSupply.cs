using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Medical supply entity - simple quantity and price
/// </summary>
public class MedicalSupply : AuditableEntity
{
    public Guid ClinicBranchId { get; set; } // Linked to branch, not clinic
    public string Name { get; set; } = null!; // Supply name only (gauze, syringes, etc.)
    
    // Simple pricing and inventory
    public decimal UnitPrice { get; set; } // Price per piece
    public int QuantityInStock { get; set; } // Simple quantity count
    public int MinimumStockLevel { get; set; } = 10;
    
    // Calculated properties
    public bool IsLowStock => QuantityInStock <= MinimumStockLevel;
    public bool HasStock => QuantityInStock > 0;


    public ClinicBranch ClinicBranch { get; set; } = null!;
    public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}
