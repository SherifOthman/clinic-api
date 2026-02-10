using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Application.DTOs;

public class InvoiceItemDto
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    
    public Guid? MedicalServiceId { get; set; }
    public Guid? MedicineId { get; set; }
    public Guid? MedicalSupplyId { get; set; }
    
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public SaleUnit? SaleUnit { get; set; }
    
    // Calculated property
    public decimal LineTotal { get; set; }
    
    // Item details for display
    public string? ItemName { get; set; }
    public string? ItemType { get; set; } // "Service", "Medicine", "Supply"
}
