using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.ValueObjects;

/// <summary>
/// Value object representing inventory stock information
/// </summary>
public record StockInfo
{
    public decimal CurrentStock { get; init; }
    public decimal MinimumStock { get; init; }
    public decimal StockPercentage { get; init; }
    public decimal StockValue { get; init; }
    public StockStatus Status { get; init; }
    public bool IsLowStock { get; init; }
    public bool IsOutOfStock { get; init; }
    
    public static StockInfo Create(
        decimal currentStock, 
        decimal minimumStock, 
        decimal unitPrice,
        StockStatus status)
    {
        var stockPercentage = minimumStock > 0 
            ? Math.Min(100, (currentStock / minimumStock) * 100) 
            : 0;
            
        var stockValue = currentStock * unitPrice;
        
        return new StockInfo
        {
            CurrentStock = currentStock,
            MinimumStock = minimumStock,
            StockPercentage = stockPercentage,
            StockValue = stockValue,
            Status = status,
            IsLowStock = status == StockStatus.LowStock,
            IsOutOfStock = status == StockStatus.OutOfStock
        };
    }
}