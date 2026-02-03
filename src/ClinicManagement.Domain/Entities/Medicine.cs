using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Medicine entity - has boxes and strips
/// </summary>
public class Medicine : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ClinicBranchId { get; set; } // Linked to branch, not clinic
    public string Name { get; set; } = null!; // Medicine name only
    
    // Pricing - boxes and strips
    public decimal BoxPrice { get; set; }
    public int StripsPerBox { get; set; } // How many strips per box
    
    // Inventory measured in strips (smallest unit)
    public int TotalStripsInStock { get; set; }
    public int MinimumStockLevel { get; set; } = 10;
    
    // Calculated properties
    public decimal StripPrice => StripsPerBox > 0 ? BoxPrice / StripsPerBox : 0;
    public int FullBoxesInStock => StripsPerBox > 0 ? TotalStripsInStock / StripsPerBox : 0;
    public int RemainingStrips => StripsPerBox > 0 ? TotalStripsInStock % StripsPerBox : TotalStripsInStock;
    public bool IsLowStock => TotalStripsInStock <= MinimumStockLevel;
    public bool HasStock => TotalStripsInStock > 0;

    // Navigation properties
    public ClinicBranch ClinicBranch { get; set; } = null!;
    public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}