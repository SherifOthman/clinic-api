using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Medicine : AuditableEntity
{
    public Guid ClinicBranchId { get; set; }
    public string? Description { get; set; }
    public string? Manufacturer { get; set; }
    public string? BatchNumber { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    
    public decimal BoxPrice { get; set; }
    public int StripsPerBox { get; set; }
    
    public int TotalStripsInStock { get; set; }
    public int MinimumStockLevel { get; set; } = 10;
    public int ReorderLevel { get; set; } = 20;
    
    public bool IsActive { get; set; } = true;
    public bool IsDiscontinued { get; set; } = false;


    public decimal StripPrice => StripsPerBox > 0 ? BoxPrice / StripsPerBox : 0;
    public int FullBoxesInStock => StripsPerBox > 0 ? TotalStripsInStock / StripsPerBox : 0;
    public int RemainingStrips => StripsPerBox > 0 ? TotalStripsInStock % StripsPerBox : TotalStripsInStock;
    public bool IsLowStock => TotalStripsInStock <= MinimumStockLevel;
    public bool NeedsReorder => TotalStripsInStock <= ReorderLevel;
    public bool HasStock => TotalStripsInStock > 0;
    public bool IsExpired(DateOnly today) => ExpiryDate.HasValue && ExpiryDate.Value < today;
    public bool IsExpiringSoon(DateOnly today) => ExpiryDate.HasValue && ExpiryDate.Value <= today.AddDays(30);
    public StockStatus GetStockStatus()
    {
        if (!HasStock) return StockStatus.OutOfStock;
        if (IsLowStock) return StockStatus.LowStock;
        if (NeedsReorder) return StockStatus.NeedsReorder;
        return StockStatus.InStock;
    }
    public int? GetDaysUntilExpiry(DateOnly today) => ExpiryDate.HasValue ? ExpiryDate.Value.DayNumber - today.DayNumber : null;
    public decimal InventoryValue => (FullBoxesInStock * BoxPrice) + (RemainingStrips * StripPrice);
}
