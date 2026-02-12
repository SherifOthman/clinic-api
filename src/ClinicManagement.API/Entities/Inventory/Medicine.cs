using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Enums;

namespace ClinicManagement.API.Entities;

public class Medicine : AuditableEntity
{
    public Guid ClinicBranchId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Manufacturer { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    
    public decimal BoxPrice { get; set; }
    public int StripsPerBox { get; set; }
    
    public int TotalStripsInStock { get; set; }
    public int MinimumStockLevel { get; set; } = 10;
    public int ReorderLevel { get; set; } = 20;
    
    public bool IsActive { get; set; } = true;
    public bool IsDiscontinued { get; set; } = false;

    // Navigation properties
    public ClinicBranch ClinicBranch { get; set; } = null!;
    public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();

    // Calculated properties
    public decimal StripPrice => StripsPerBox > 0 ? BoxPrice / StripsPerBox : 0;
    public int FullBoxesInStock => StripsPerBox > 0 ? TotalStripsInStock / StripsPerBox : 0;
    public int RemainingStrips => StripsPerBox > 0 ? TotalStripsInStock % StripsPerBox : TotalStripsInStock;
    public bool IsLowStock => TotalStripsInStock <= MinimumStockLevel;
    public bool NeedsReorder => TotalStripsInStock <= ReorderLevel;
    public bool HasStock => TotalStripsInStock > 0;
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value.Date < DateTime.UtcNow.Date;
    public bool IsExpiringSoon => ExpiryDate.HasValue && ExpiryDate.Value.Date <= DateTime.UtcNow.Date.AddDays(30);
    public StockStatus StockStatus
    {
        get
        {
            if (!HasStock) return StockStatus.OutOfStock;
            if (IsLowStock) return StockStatus.LowStock;
            if (NeedsReorder) return StockStatus.NeedsReorder;
            return StockStatus.InStock;
        }
    }
    public int? DaysUntilExpiry => ExpiryDate?.Subtract(DateTime.UtcNow.Date).Days;
    public decimal InventoryValue => (FullBoxesInStock * BoxPrice) + (RemainingStrips * StripPrice);
}
