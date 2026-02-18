using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class MedicalService : AuditableEntity
{
    public Guid ClinicBranchId { get; set; } // Linked to branch, not clinic
    public decimal DefaultPrice { get; set; }
    public bool IsOperation { get; set; } // Is it a surgical operation?
}
