namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Audit trail for user role assignments and removals.
/// Tracks when roles are added or removed, who made the change, and the reason.
/// </summary>
public class UserRoleHistory
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public string Action { get; set; } = string.Empty; // "Added" or "Removed"
    public DateTime ChangedAt { get; set; }
    public Guid ChangedBy { get; set; }
    public string? Reason { get; set; }
}
