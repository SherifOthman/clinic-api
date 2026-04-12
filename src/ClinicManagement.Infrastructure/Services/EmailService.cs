using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Common.Options;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SmtpEmailSender _smtpEmailSender;
    private readonly AppOptions      _appOptions;

    public EmailService(SmtpEmailSender smtpEmailSender, IOptions<AppOptions> appOptions)
    {
        _smtpEmailSender = smtpEmailSender;
        _appOptions      = appOptions.Value;
    }

    public async Task SendPasswordResetEmailAsync(
        string toEmail, string userName, string resetLink,
        CancellationToken cancellationToken = default)
    {
        var emailBody = EmailTemplates.GetPasswordResetTemplate(userName, resetLink);
        await _smtpEmailSender.SendEmailAsync(toEmail, "Reset your password", emailBody, cancellationToken);
    }

    public async Task SendStaffInvitationEmailAsync(
        string toEmail, string clinicName, string role, string invitedBy,
        string invitationLink, CancellationToken cancellationToken = default)
    {
        var fullLink  = $"{_appOptions.FrontendUrl}{invitationLink}";
        var emailBody = EmailTemplates.GetStaffInvitationTemplate(clinicName, role, invitedBy, fullLink);
        await _smtpEmailSender.SendEmailAsync(toEmail, $"Invitation to join {clinicName} as {role}", emailBody, cancellationToken);
    }

    public async Task SendEmailAsync(
        string toEmail, string? toName, string subject, string body,
        bool isHtml = true, CancellationToken cancellationToken = default)
    {
        await _smtpEmailSender.SendEmailAsync(toEmail, subject, body, cancellationToken);
    }
}
