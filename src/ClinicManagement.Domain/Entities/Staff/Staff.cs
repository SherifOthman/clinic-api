using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Staff : TenantEntity
{
    public Guid UserId { get; init; }
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; private set; }
    
    // Employment tracking (US-2, US-8)
    public bool IsPrimaryClinic { get; set; } = false;
    public StaffStatus Status { get; set; } = StaffStatus.Active;
    public DateTime? StatusChangedAt { get; set; }
    public Guid? StatusChangedBy { get; set; }
    public string? StatusReason { get; set; }
    public DateTime? TerminationDate { get; set; }
}
