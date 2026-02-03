using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class MedicalService : AuditableEntity
{
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public ServiceType Type { get; set; } // Consultation, Operation, Lab Test, etc.
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Clinic Clinic { get; set; } = null!;
    public ICollection<TransactionService> TransactionServices { get; set; } = new List<TransactionService>();
}