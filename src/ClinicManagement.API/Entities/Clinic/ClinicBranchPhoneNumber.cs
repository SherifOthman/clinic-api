using ClinicManagement.API.Common;

namespace ClinicManagement.API.Entities;

public class ClinicBranchPhoneNumber : BaseEntity
{
    public Guid ClinicBranchId { get; set; }
    public ClinicBranch ClinicBranch { get; set; } = null!;
    
    public string PhoneNumber { get; set; } = null!;
    
    public string? Label { get; set; }
}
