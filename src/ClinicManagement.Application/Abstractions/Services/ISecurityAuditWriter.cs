namespace ClinicManagement.Application.Abstractions.Services;

/// <summary>
/// Writes security-related audit log entries (login, logout, failed attempts, lockouts).
/// Abstracts the audit persistence so auth handlers don't depend on the DbContext directly
/// for audit writes, and so the logic isn't duplicated across handlers.
/// </summary>
public interface ISecurityAuditWriter
{
    /// <summary>
    /// Writes a security audit entry and persists it immediately.
    /// </summary>
    /// <param name="userId">The user involved. Null for unknown-user events (e.g. login with bad email).</param>
    /// <param name="fullName">Display name at time of event.</param>
    /// <param name="username">Login username.</param>
    /// <param name="userEmail">Email address.</param>
    /// <param name="userRole">Role(s) at time of event.</param>
    /// <param name="clinicId">Clinic context. Null for system-level or pre-auth events.</param>
    /// <param name="eventName">Event key, e.g. "LoginSuccess", "LoginFailed", "AccountLocked", "Logout".</param>
    /// <param name="detail">Optional extra detail appended to the Changes JSON.</param>
    /// <param name="cancellationToken"></param>
    Task WriteAsync(
        Guid? userId,
        string? fullName,
        string? username,
        string? userEmail,
        string? userRole,
        Guid? clinicId,
        string eventName,
        string? detail = null,
        CancellationToken cancellationToken = default);
}
