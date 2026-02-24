using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class StaffBranch : BaseEntity
{
    public Guid StaffId { get; set; }
    public Guid ClinicBranchId { get; set; }
    public bool IsPrimaryBranch { get; set; } = false;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; set; }
}
