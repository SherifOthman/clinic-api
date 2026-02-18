namespace ClinicManagement.Domain.Enums;

/// <summary>
/// Represents the stock status of inventory items
/// </summary>
public enum StockStatus
{
    /// <summary>
    /// Item is in stock with adequate quantity
    /// </summary>
    InStock = 1,
    
    /// <summary>
    /// Item stock is below minimum level but still available
    /// </summary>
    LowStock = 2,
    
    /// <summary>
    /// Item stock has reached reorder level
    /// </summary>
    NeedsReorder = 3,
    
    /// <summary>
    /// Item is completely out of stock
    /// </summary>
    OutOfStock = 4
}
