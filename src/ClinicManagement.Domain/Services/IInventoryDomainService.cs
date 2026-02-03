using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Services;

/// <summary>
/// Domain service for Inventory business logic
/// </summary>
public interface IInventoryDomainService
{
    /// <summary>
    /// Checks if an inventory item is low on stock
    /// </summary>
    bool IsLowStock(InventoryItem item);
    
    /// <summary>
    /// Checks if an inventory item is out of stock
    /// </summary>
    bool IsOutOfStock(InventoryItem item);
    
    /// <summary>
    /// Calculates the stock status percentage (0-100%)
    /// </summary>
    decimal CalculateStockPercentage(InventoryItem item);
    
    /// <summary>
    /// Determines if a quantity can be consumed from stock
    /// </summary>
    bool CanConsumeQuantity(InventoryItem item, decimal quantity);
    
    /// <summary>
    /// Calculates the total value of current stock
    /// </summary>
    decimal CalculateStockValue(InventoryItem item);
    
    /// <summary>
    /// Gets the stock status as an enum
    /// </summary>
    StockStatus GetStockStatus(InventoryItem item);
}