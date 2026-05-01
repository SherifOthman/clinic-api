using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Application.Abstractions.Services;

/// <summary>
/// Single entry point for writing audit log entries.
///
/// Two usage patterns:
///
/// 1. Automatic — SaveChanges interceptor calls WriteEntityChangeAsync for every
///    IAuditableEntity that was Added/Modified/Deleted. Field-level diffs are
///    captured automatically. Handlers don't need to do anything.
///
/// 2. Manual — handlers call WriteEventAsync for business events that have no
///    entity diff (login, logout, password change, permissions, invitations).
///    The handler provides the event name and optional human-readable detail.
/// </summary>
public interface IAuditWriter
{
    /// <summary>
    /// Write a business event that has no entity diff.
    /// Used for: login, logout, password change, permissions, invitations.
    /// Current user context (userId, name, email, role, IP, userAgent) is
    /// read automatically from ICurrentUserService — no need to pass them.
    /// </summary>
    Task WriteEventAsync(
        string eventName,
        string? detail = null,
        Guid? overrideUserId = null,
        string? overrideFullName = null,
        string? overrideEmail = null,
        string? overrideRole = null,
        Guid? overrideClinicId = null,
        CancellationToken ct = default);
}
