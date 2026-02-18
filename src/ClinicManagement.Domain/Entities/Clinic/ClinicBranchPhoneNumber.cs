using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ClinicBranchPhoneNumber : BaseEntity
{
    public int ClinicBranchId { get; set; }
    
    public string? Label { get; set; }
}
