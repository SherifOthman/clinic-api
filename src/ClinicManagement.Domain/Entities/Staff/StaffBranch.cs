using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Assigns staff to specific branches within a clinic.
/// Supports multi-branch assignment with one primary branch per staff member.
/// </summary>
public class StaffBranch : BaseEntity
{
    public Guid StaffId { get; set; }
    public Guid ClinicBranchId { get; set; }
    public bool IsPrimaryBranch { get; set; } = false;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
