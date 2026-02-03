using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

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
    
    // Navigation properties
    public Clinic Clinic { get; set; } = null!;
    public ICollection<TransactionItem> TransactionItems { get; set; } = new List<TransactionItem>();
}