using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Services;

/// <summary>
/// Domain service implementing Inventory business logic
/// </summary>
public class InventoryDomainService : IInventoryDomainService
{
    public bool IsLowStock(InventoryItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
            
        if (!item.CurrentStock.HasValue || !item.MinimumStock.HasValue)
            return false;
            
        return item.CurrentStock.Value <= item.MinimumStock.Value;
    }
    
    public bool IsOutOfStock(InventoryItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
            
        return !item.CurrentStock.HasValue || item.CurrentStock.Value <= 0;
    }
    
    public decimal CalculateStockPercentage(InventoryItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
            
        if (!item.CurrentStock.HasValue || !item.MinimumStock.HasValue || item.MinimumStock.Value == 0)
            return 0;
            
        return Math.Min(100, (item.CurrentStock.Value / item.MinimumStock.Value) * 100);
    }
    
    public bool CanConsumeQuantity(InventoryItem item, decimal quantity)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
            
        if (!item.CurrentStock.HasValue || quantity <= 0)
            return false;
            
        return item.CurrentStock.Value >= quantity;
    }
    
    public decimal CalculateStockValue(InventoryItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
            
        if (!item.CurrentStock.HasValue || !item.UnitPrice.HasValue)
            return 0;
            
        return item.CurrentStock.Value * item.UnitPrice.Value;
    }
    
    public StockStatus GetStockStatus(InventoryItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
            
        if (IsOutOfStock(item))
            return StockStatus.OutOfStock;
            
        if (IsLowStock(item))
            return StockStatus.LowStock;
            
        return StockStatus.InStock;
    }
}