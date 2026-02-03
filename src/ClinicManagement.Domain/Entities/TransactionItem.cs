namespace ClinicManagement.Domain.Entities;

public class TransactionItem
{
    public Guid Id { get; set; }
    public Guid PatientTransactionId { get; set; }
    public Guid InventoryItemId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // Price at the time of sale
    
    // Navigation properties
    public PatientTransaction PatientTransaction { get; set; } = null!;
    public InventoryItem InventoryItem { get; set; } = null!;
}