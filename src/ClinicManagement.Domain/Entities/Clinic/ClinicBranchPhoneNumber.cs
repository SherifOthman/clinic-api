using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ClinicBranchPhoneNumber : BaseEntity
{
    public Guid ClinicBranchId { get; set; }
    public ClinicBranch ClinicBranch { get; set; } = null!;
    
    public string PhoneNumber { get; set; } = null!;
    
    public string? Label { get; set; }
}
