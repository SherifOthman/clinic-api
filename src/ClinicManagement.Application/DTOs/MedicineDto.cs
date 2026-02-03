namespace ClinicManagement.Application.DTOs;

public class MedicineDto
{
    public Guid Id { get; set; }
    public Guid ClinicBranchId { get; set; }
    public string Name { get; set; } = null!;
    public decimal BoxPrice { get; set; }
    public int StripsPerBox { get; set; }
    public int TotalStripsInStock { get; set; }
    public int MinimumStockLevel { get; set; }
    
    // Calculated properties
    public decimal StripPrice { get; set; }
    public int FullBoxesInStock { get; set; }
    public int RemainingStrips { get; set; }
    public bool IsLowStock { get; set; }
    public bool HasStock { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
