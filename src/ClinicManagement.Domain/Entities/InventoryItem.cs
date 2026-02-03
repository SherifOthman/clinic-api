using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Inventory items for the clinic (medicines, medical supplies).
/// Supports quantity-based billing for pharmacy items.
/// </summary>
public class InventoryItem : AuditableEntity
{
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public InventoryItemType Type { get; set; } // Medicine / Medical Supply
    public string Unit { get; set; } = "piece";
    public decimal? CurrentStock { get; set; }
    public decimal? MinimumStock { get; set; }
    public decimal? UnitPrice { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Additional inventory fields
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Supplier { get; set; }
    
    // Navigation properties
    public Clinic Clinic { get; set; } = null!;
    public ICollection<VisitServiceItem> VisitServiceItems { get; set; } = new List<VisitServiceItem>();
    
    // Legacy - keeping for backward compatibility
    [Obsolete("Use VisitServiceItems instead")]
    public ICollection<TransactionItem> TransactionItems { get; set; } = new List<TransactionItem>();
}