using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class StaffInvitation : AuditableEntity
{
    public string Email { get; set; } = null!;
    public UserType UserType { get; set; } // Doctor or Receptionist
    
    public Guid ClinicId { get; set; }
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
