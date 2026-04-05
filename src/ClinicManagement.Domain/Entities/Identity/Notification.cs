using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// User notification with read/unread status.
/// </summary>
public class Notification : AuditableEntity, INoAuditLog
{
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; } = NotificationType.Info;
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
