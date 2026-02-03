namespace ClinicManagement.Application.DTOs;

public class MedicalServiceDto
{
    public Guid Id { get; set; }
    public Guid ClinicBranchId { get; set; }
    public string Name { get; set; } = null!;
    public decimal DefaultPrice { get; set; }
    public bool IsOperation { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
