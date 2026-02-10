namespace ClinicManagement.Application.DTOs;

public class MedicalSupplyDto
{
    public Guid Id { get; set; }
    public Guid ClinicBranchId { get; set; }
    public string Name { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int QuantityInStock { get; set; }
    public int MinimumStockLevel { get; set; }
    
    // Calculated properties
    public bool IsLowStock { get; set; }
    public bool HasStock { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
