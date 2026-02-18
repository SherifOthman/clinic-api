using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Abstractions.Storage;

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

