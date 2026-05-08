using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Common.Options;
using ClinicManagement.Infrastructure.Services.Emails;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Sends emails via SMTP. Delegates template building to IEmail implementations.
///
/// Adding a new email type = create a new IEmail class in Services/Emails/.
/// EmailService itself never needs to change (OCP).
/// </summary>
public class EmailService : IEmailService
{
    private readonly SmtpEmailSender _smtp;
    private readonly AppOptions      _appOptions;

    public EmailService(SmtpEmailSender smtp, IOptions<AppOptions> appOptions)
    {
        _smtp       = smtp;
        _appOptions = appOptions.Value;
    }

    // ── Preferred entry point ─────────────────────────────────────────────────

    /// <summary>
    /// Send any IEmail implementation. This is the single place that touches SMTP.
    /// All named methods below delegate here.
    /// </summary>
    public Task SendAsync(IEmail email, CancellationToken cancellationToken = default)
        => _smtp.SendEmailAsync(email.ToEmail, email.Subject, email.Body, cancellationToken);

    // ── Named convenience methods (backward compatible) ───────────────────────

    public Task SendPasswordResetEmailAsync(
        string toEmail, string userName, string resetLink,
        CancellationToken cancellationToken = default)
        => SendAsync(new PasswordResetEmail(toEmail, userName, resetLink), cancellationToken);

    public Task SendStaffInvitationEmailAsync(
        string toEmail, string clinicName, string role, string invitedBy,
        string invitationLink, CancellationToken cancellationToken = default)
    {
        var fullLink = $"{_appOptions.WebsiteUrl}{invitationLink}";
        return SendAsync(new StaffInvitationEmail(toEmail, clinicName, role, invitedBy, fullLink), cancellationToken);
    }

    public Task SendEmailAsync(
        string toEmail, string? toName, string subject, string body,
        bool isHtml = true, CancellationToken cancellationToken = default)
        => _smtp.SendEmailAsync(toEmail, subject, body, cancellationToken);
}
