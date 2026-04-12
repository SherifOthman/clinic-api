using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Immutable audit log entry. Never updated or soft-deleted — only appended.
/// Captures every Create/Update/Delete across all auditable entities.
/// SuperAdmin uses this to debug issues across all clinics.
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>UTC timestamp of the action.</summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>The clinic this action belongs to. Null for system-level actions (SuperAdmin).</summary>
    public Guid? ClinicId { get; set; }

    /// <summary>The user who performed the action. Null for system/seed operations.</summary>
    public Guid? UserId { get; set; }

    /// <summary>Full display name of the user at the time of the action.</summary>
    public string? FullName { get; set; }

    /// <summary>Login username — stable identifier for searching.</summary>
    public string? Username { get; set; }

    /// <summary>Email of the user — globally unique identifier across all clinics.</summary>
    public string? UserEmail { get; set; }

    /// <summary>Role of the user at the time (e.g. Doctor, Receptionist).</summary>
    public string? UserRole { get; set; }

    /// <summary>Browser/device info — critical for security debugging.</summary>
    public string? UserAgent { get; set; }

    /// <summary>The entity type name (e.g. "Patient", "Staff").</summary>
    public string EntityType { get; set; } = null!;

    /// <summary>The primary key of the affected entity.</summary>
    public string EntityId { get; set; } = null!;

    /// <summary>Create | Update | Delete | Security | Restore</summary>
    public AuditAction Action { get; set; }

    /// <summary>IP address of the request.</summary>
    public string? IpAddress { get; set; }

    /// <summary>JSON of changed fields: { "FieldName": { "Old": "...", "New": "..." } }</summary>
    public string? Changes { get; set; }
}
