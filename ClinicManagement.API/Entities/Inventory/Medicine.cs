using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Common.Exceptions;
using ClinicManagement.API.Common.Constants;

namespace ClinicManagement.API.Entities;

/// <summary>
/// Medicine aggregate root - manages inventory in boxes and strips with comprehensive business logic
/// Enforces business rules and maintains consistency
/// </summary>
public class Medicine : AuditableEntity
{
    // Private constructor for EF Core
    private Medicine() { }

    public Guid ClinicBranchId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? Manufacturer { get; private set; }
    public string? BatchNumber { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    
    // Pricing - boxes and strips
    public decimal BoxPrice { get; private set; }
    public int StripsPerBox { get; private set; }
    
    // Inventory measured in strips (smallest unit)
    public int TotalStripsInStock { get; private set; }
    public int MinimumStockLevel { get; private set; } = 10;
    public int ReorderLevel { get; private set; } = 20;
    
    // Status
    public bool IsActive { get; private set; } = true;
    public bool IsDiscontinued { get; private set; } = false;

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
    /// Factory method to create a new medicine
    /// Ensures all invariants are met and raises domain event
    /// </summary>
    public static Medicine Create(
        Guid clinicBranchId,
        string name,
        decimal boxPrice,
        int stripsPerBox,
        int initialStock = 0,
        string? description = null,
        string? manufacturer = null,
        string? batchNumber = null,
        DateTime? expiryDate = null,
        int minimumStockLevel = 10,
        int reorderLevel = 20)
    {
        // Validate inputs
        if (clinicBranchId == Guid.Empty)
            throw new InvalidBusinessOperationException("Clinic branch ID is required");
        
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidBusinessOperationException("Medicine name is required");
        
        if (boxPrice <= 0)
            throw new InvalidBusinessOperationException("Box price must be positive");
        
        if (stripsPerBox <= 0)
            throw new InvalidBusinessOperationException("Strips per box must be positive");
        
        if (initialStock < 0)
            throw new InvalidBusinessOperationException("Initial stock cannot be negative");
        
        if (minimumStockLevel < 0)
            throw new InvalidBusinessOperationException("Minimum stock level cannot be negative");
        
        if (reorderLevel < minimumStockLevel)
            throw new InvalidBusinessOperationException("Reorder level cannot be less than minimum stock level");
        
        if (expiryDate.HasValue && expiryDate.Value.Date <= DateTime.UtcNow.Date)
            throw new InvalidBusinessOperationException("Expiry date must be in the future");

        var medicine = new Medicine
        {
            ClinicBranchId = clinicBranchId,
            Name = name,
            Description = description,
            Manufacturer = manufacturer,
            BatchNumber = batchNumber,
            ExpiryDate = expiryDate,
            BoxPrice = boxPrice,
            StripsPerBox = stripsPerBox,
            TotalStripsInStock = initialStock,
            MinimumStockLevel = minimumStockLevel,
            ReorderLevel = reorderLevel,
            IsActive = true,
            IsDiscontinued = false
        };

        return medicine;
    }
    
    /// <summary>
    /// Adds stock to the medicine inventory
    /// </summary>
    /// <param name="strips">Number of strips to add</param>
    /// <param name="reason">Reason for stock addition</param>
    /// <exception cref="InvalidBusinessOperationException">Thrown when strips is not positive or medicine is discontinued</exception>
    public void AddStock(int strips, string? reason = null)
    {
        if (strips <= 0) 
            throw new InvalidBusinessOperationException("Strips to add must be positive");
        if (IsDiscontinued) 
            throw new DiscontinuedMedicineException("Cannot add stock to discontinued medicine");
        
        TotalStripsInStock += strips;
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
            throw new InvalidBusinessOperationException("Strips to remove must be positive");
        if (strips > TotalStripsInStock) 
            throw new InsufficientStockException(strips, TotalStripsInStock);
        
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
            throw new InvalidBusinessOperationException("Strips must be positive");
        
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
            throw new InvalidBusinessOperationException("Boxes and strips cannot be negative");
        
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
    }
    
    /// <summary>
    /// Reactivates a discontinued medicine
    /// </summary>
    /// <exception cref="ExpiredMedicineException">Thrown when medicine is expired</exception>
    public void Reactivate()
    {
        if (IsExpired && ExpiryDate.HasValue) 
            throw new ExpiredMedicineException(ExpiryDate.Value);
        
        IsDiscontinued = false;
        IsActive = true;
    }

    /// <summary>
    /// Updates medicine information
    /// </summary>
    public void UpdateInfo(
        string name,
        decimal boxPrice,
        int stripsPerBox,
        int minimumStockLevel,
        int? reorderLevel = null,
        string? description = null,
        string? manufacturer = null,
        string? batchNumber = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidBusinessOperationException("Medicine name is required");
        
        if (boxPrice <= 0)
            throw new InvalidBusinessOperationException("Box price must be positive");
        
        if (stripsPerBox <= 0)
            throw new InvalidBusinessOperationException("Strips per box must be positive");
        
        if (minimumStockLevel < 0)
            throw new InvalidBusinessOperationException("Minimum stock level cannot be negative");
        
        var effectiveReorderLevel = reorderLevel ?? minimumStockLevel * 2;
        if (effectiveReorderLevel < minimumStockLevel)
            throw new InvalidBusinessOperationException("Reorder level cannot be less than minimum stock level");

        Name = name;
        BoxPrice = boxPrice;
        StripsPerBox = stripsPerBox;
        MinimumStockLevel = minimumStockLevel;
        ReorderLevel = effectiveReorderLevel;
        Description = description;
        Manufacturer = manufacturer;
        BatchNumber = batchNumber;
    }
    
    /// <summary>
    /// Updates the expiry date and validates it
    /// </summary>
    /// <param name="newExpiryDate">New expiry date</param>
    /// <exception cref="InvalidBusinessOperationException">Thrown when expiry date is in the past</exception>
    public void UpdateExpiryDate(DateTime newExpiryDate)
    {
        if (newExpiryDate.Date <= DateTime.UtcNow.Date)
            throw new InvalidBusinessOperationException("Expiry date must be in the future");
            
        ExpiryDate = newExpiryDate;
    }
    
    /// <summary>
    /// Validates the medicine entity
    /// </summary>
    /// <exception cref="InvalidBusinessOperationException">Thrown when validation fails</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidBusinessOperationException("Medicine name is required");
            
        if (BoxPrice <= 0)
            throw new InvalidBusinessOperationException("Box price must be positive");
            
        if (StripsPerBox <= 0)
            throw new InvalidBusinessOperationException("Strips per box must be positive");
            
        if (TotalStripsInStock < 0)
            throw new InvalidBusinessOperationException("Stock cannot be negative");
            
        if (MinimumStockLevel < 0)
            throw new InvalidBusinessOperationException("Minimum stock level cannot be negative");
            
        if (ReorderLevel < MinimumStockLevel)
            throw new InvalidBusinessOperationException("Reorder level cannot be less than minimum stock level");
    }
}
