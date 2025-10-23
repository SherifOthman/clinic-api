using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class DoctorBranch : BaseEntity
{
    public int DoctorId { get; set; }
    public int BranchId { get; set; }
    
    // Navigation properties
    public virtual Doctor Doctor { get; set; } = null!;
    public virtual ClinicBranch Branch { get; set; } = null!;
}
