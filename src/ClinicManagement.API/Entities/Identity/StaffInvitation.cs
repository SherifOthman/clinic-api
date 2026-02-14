using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Enums;

namespace ClinicManagement.API.Entities;

public class StaffInvitation : TenantEntity
{
    public string Email { get; set; } = null!;
    public UserType UserType { get; set; } // Doctor or Receptionist
    
    public Clinic Clinic { get; set; } = null!;
    
    public Guid InvitedByUserId { get; set; }
    public User InvitedByUser { get; set; } = null!;
    
    public string Token { get; set; } = null!; // Unique token for invitation link
    public DateTime ExpiresAt { get; set; }
    public bool IsAccepted { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public Guid? AcceptedByUserId { get; set; }
    public User? AcceptedByUser { get; set; }
}
