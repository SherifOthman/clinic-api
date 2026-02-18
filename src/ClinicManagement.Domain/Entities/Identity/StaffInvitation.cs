using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class StaffInvitation : TenantEntity
{
    public string Role { get; set; } = null!; // Doctor or Receptionist (ASP.NET Identity role name)
    
    public int InvitedByUserId { get; set; }
    
    public string Token { get; set; } = null!; // Unique token for invitation link
    public DateTime ExpiresAt { get; set; }
    public bool IsAccepted { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public int? AcceptedByUserId { get; set; }
    public User? AcceptedByUser { get; set; }
}
