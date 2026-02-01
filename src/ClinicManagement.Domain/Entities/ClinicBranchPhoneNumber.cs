using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ClinicBranchPhoneNumber : BaseEntity
{
    public Guid ClinicBranchId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Label { get; set; } // e.g., "Main", "Emergency", "Reception"
    
    // Navigation properties
    public virtual ClinicBranch ClinicBranch { get; set; } = null!;
}