namespace ClinicManagement.Application.Abstractions.Services;

/// <summary>
/// Marker interface for commands that should be recorded in the audit log.
/// The behavior writes one AuditLog entry after the handler succeeds.
/// AuditEvent  — the event name stored in the log (e.g. "PasswordChanged").
/// AuditDetail — optional human-readable context (e.g. "Invited john@clinic.com as Doctor").
///               Keep it free of sensitive data (no passwords, no tokens).
/// </summary>
public interface IAuditableCommand
{
    string AuditEvent  { get; }
    string? AuditDetail { get; }
}
