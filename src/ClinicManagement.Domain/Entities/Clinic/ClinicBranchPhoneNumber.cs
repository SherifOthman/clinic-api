using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ClinicBranchPhoneNumber : BaseEntity
{
    public Guid ClinicBranchId { get; set; }
    
    public string? Label { get; set; }
}
