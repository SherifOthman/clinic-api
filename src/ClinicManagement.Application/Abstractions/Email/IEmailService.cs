namespace ClinicManagement.Application.Abstractions.Email;

public interface IEmailService
{
    /// <summary>
    /// Send a fully-formed email object.
    /// Preferred over the named methods — adding a new email type means
    /// creating a new IEmail implementation, not adding a method here (OCP).
    /// </summary>
    Task SendAsync(IEmail email, CancellationToken cancellationToken = default);

    // ── Named convenience methods (kept for backward compatibility) ───────────

    Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink, CancellationToken cancellationToken = default);
    Task SendStaffInvitationEmailAsync(string toEmail, string clinicName, string role, string invitedBy, string invitationLink, CancellationToken cancellationToken = default);
    Task SendEmailAsync(string toEmail, string? toName, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
}
