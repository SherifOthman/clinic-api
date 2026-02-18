using ClinicManagement.Application.Common.Interfaces;

namespace ClinicManagement.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SmtpEmailSender _smtpEmailSender;

    public EmailService(SmtpEmailSender smtpEmailSender)
    {
        _smtpEmailSender = smtpEmailSender;
    }

    public async Task SendPasswordResetEmailAsync(
        string toEmail,
        string userName,
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        var emailBody = EmailTemplates.GetPasswordResetTemplate(userName, resetLink);
        await _smtpEmailSender.SendEmailAsync(
            toEmail,
            "Reset your password",
            emailBody,
            cancellationToken);
    }
}
