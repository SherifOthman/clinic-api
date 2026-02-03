using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Exceptions;
using ClinicManagement.Domain.Common.Constants;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Medicine entity - manages inventory in boxes and strips with comprehensive business logic
/// </summary>
public class Medicine : AuditableEntity
{
    public Guid ClinicBranchId { get; set; } // Linked to branch, not clinic
    public string Name { get; set; } = null!; // Medicine name only
    public string? Description { get; set; }
    public string? Manufacturer { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    
    // Pricing - boxes and strips
    public decimal BoxPrice { get; set; }
    public int StripsPerBox { get; set; } // How many strips per box
    
    // Inventory measured in strips (smallest unit)
    public int TotalStripsInStock { get; set; }
    public int MinimumStockLevel { get; set; } = 10;
    public int ReorderLevel { get; set; } = 20;
    
    // Status
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
    
    /// <summary>
    /// Gets the stock status as an enum
    /// </summary>
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
    
    /// <summary>
    /// Gets the number of days until expiry
    /// </summary>
    public int? DaysUntilExpiry => ExpiryDate?.Subtract(DateTime.UtcNow.Date).Days;
    
    /// <summary>
    /// Calculates the total inventory value
    /// </summary>
    public decimal InventoryValue => (FullBoxesInStock * BoxPrice) + (RemainingStrips * StripPrice);
    
    /// <summary>
    /// Adds stock to the medicine inventory
    /// </summary>
    /// <param name="strips">Number of strips to add</param>
    /// <param name="reason">Reason for stock addition</param>
    /// <exception cref="InvalidBusinessOperationException">Thrown when strips is not positive or medicine is discontinued</exception>
    public void AddStock(int strips, string? reason = null)
    {
        if (strips <= 0) 
            throw new InvalidBusinessOperationException("Strips to add must be positive", MessageCodes.Domain.INVALID_BUSINESS_OPERATION);
        if (IsDiscontinued) 
            throw new DiscontinuedMedicineException(MessageCodes.Domain.DISCONTINUED_MEDICINE);
        
        TotalStripsInStock += strips;
        
        // Log the stock movement (this could be expanded to a separate StockMovement entity)
        // For now, we'll just update the stock
    }
    
    /// <summary>
    /// Removes stock from the medicine inventory
    /// </summary>
    /// <param name="strips">Number of strips to remove</param>
    /// <param name="reason">Reason for stock removal</param>
    /// <exception cref="InvalidBusinessOperationException">Thrown when strips is not positive</exception>
    /// <exception cref="InsufficientStockException">Thrown when there's not enough stock</exception>
    public void RemoveStock(int strips, string? reason = null)
    {
        if (strips <= 0) 
            throw new InvalidBusinessOperationException("Strips to remove must be positive", MessageCodes.Domain.INVALID_BUSINESS_OPERATION);
        if (strips > TotalStripsInStock) 
            throw new InsufficientStockException(strips, TotalStripsInStock, MessageCodes.Domain.INSUFFICIENT_STOCK);
        
        TotalStripsInStock -= strips;
    }
    
    /// <summary>
    /// Checks if the requested quantity is available in stock
    /// </summary>
    /// <param name="requestedStrips">Number of strips requested</param>
    /// <returns>True if available, false otherwise</returns>
    public bool IsQuantityAvailable(int requestedStrips)
    {
        return requestedStrips > 0 && requestedStrips <= TotalStripsInStock && IsActive && !IsDiscontinued;
    }
    
    /// <summary>
    /// Calculates the price for a given quantity of strips
    /// </summary>
    /// <param name="strips">Number of strips</param>
    /// <returns>Total price</returns>
    /// <exception cref="InvalidBusinessOperationException">Thrown when strips is not positive</exception>
    public decimal CalculatePrice(int strips)
    {
        if (strips <= 0) 
            throw new InvalidBusinessOperationException("Strips must be positive", MessageCodes.Domain.INVALID_BUSINESS_OPERATION);
        
        return strips * StripPrice;
    }
    
    /// <summary>
    /// Calculates the price for boxes and strips combination
    /// </summary>
    /// <param name="boxes">Number of boxes</param>
    /// <param name="strips">Number of additional strips</param>
    /// <returns>Total price</returns>
    /// <exception cref="InvalidBusinessOperationException">Thrown when boxes or strips are negative</exception>
    public decimal CalculatePrice(int boxes, int strips)
    {
        if (boxes < 0 || strips < 0) 
            throw new InvalidBusinessOperationException("Boxes and strips cannot be negative", MessageCodes.Domain.INVALID_BUSINESS_OPERATION);
        
        return (boxes * BoxPrice) + (strips * StripPrice);
    }
    
    /// <summary>
    /// Marks the medicine as discontinued
    /// </summary>
    /// <param name="reason">Reason for discontinuation</param>
    public void Discontinue(string? reason = null)
    {
        IsDiscontinued = true;
        IsActive = false;
        
        // Could log the reason in a separate audit table
    }
    
    /// <summary>
    /// Reactivates a discontinued medicine
    /// </summary>
    /// <exception cref="ExpiredMedicineException">Thrown when medicine is expired</exception>
    public void Reactivate()
    {
        if (IsExpired && ExpiryDate.HasValue) 
            throw new ExpiredMedicineException(ExpiryDate.Value, MessageCodes.Domain.EXPIRED_MEDICINE);
        
        IsDiscontinued = false;
        IsActive = true;
    }
    
    /// <summary>
    /// Updates the expiry date and validates it
    /// </summary>
    /// <param name="newExpiryDate">New expiry date</param>
    /// <exception cref="InvalidBusinessOperationException">Thrown when expiry date is in the past</exception>
    public void UpdateExpiryDate(DateTime newExpiryDate)
    {
        if (newExpiryDate.Date <= DateTime.UtcNow.Date)
            throw new InvalidBusinessOperationException("Expiry date must be in the future", MessageCodes.Domain.INVALID_EXPIRY_DATE);
            
        ExpiryDate = newExpiryDate;
    }
    
    /// <summary>
    /// Validates the medicine entity
    /// </summary>
    /// <exception cref="InvalidBusinessOperationException">Thrown when validation fails</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidBusinessOperationException("Medicine name is required", MessageCodes.Domain.MEDICINE_VALIDATION_FAILED);
            
        if (BoxPrice <= 0)
            throw new InvalidBusinessOperationException("Box price must be positive", MessageCodes.Domain.MEDICINE_VALIDATION_FAILED);
            
        if (StripsPerBox <= 0)
            throw new InvalidBusinessOperationException("Strips per box must be positive", MessageCodes.Domain.MEDICINE_VALIDATION_FAILED);
            
        if (TotalStripsInStock < 0)
            throw new InvalidBusinessOperationException("Stock cannot be negative", MessageCodes.Domain.MEDICINE_VALIDATION_FAILED);
            
        if (MinimumStockLevel < 0)
            throw new InvalidBusinessOperationException("Minimum stock level cannot be negative", MessageCodes.Domain.MEDICINE_VALIDATION_FAILED);
            
        if (ReorderLevel < MinimumStockLevel)
            throw new InvalidBusinessOperationException("Reorder level cannot be less than minimum stock level", MessageCodes.Domain.MEDICINE_VALIDATION_FAILED);
    }
}
