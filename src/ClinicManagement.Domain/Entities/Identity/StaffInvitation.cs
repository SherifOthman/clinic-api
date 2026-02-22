using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class StaffInvitation : TenantEntity
{
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string InvitationToken { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool IsAccepted { get; set; }
    public bool IsCanceled { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public int? AcceptedByUserId { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
