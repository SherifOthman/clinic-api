using ClinicManagement.Application.Common.Options;
using ClinicManagement.Infrastructure.Options;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ClinicManagement.Infrastructure.Services;

public class SmtpEmailSender
{
    private readonly SmtpOptions _smtpOptions;
    private readonly AppOptions  _appOptions;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(
        IOptions<SmtpOptions> smtpOptions,
        IOptions<AppOptions> appOptions,
        ILogger<SmtpEmailSender> logger)
    {
        _smtpOptions = smtpOptions.Value;
        _appOptions  = appOptions.Value;
        _logger      = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending email to {Email} with subject: {Subject}", toEmail, subject);

            // Replace frontend URL placeholder in email templates
            htmlMessage = htmlMessage.Replace("{{FRONTEND_URL}}", _appOptions.FrontendUrl);

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_smtpOptions.FromName, _smtpOptions.FromEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body    = new BodyBuilder { HtmlBody = htmlMessage }.ToMessageBody();

            using var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port,
                MailKit.Security.SecureSocketOptions.StartTls, cancellationToken);
            await smtpClient.AuthenticateAsync(_smtpOptions.UserName, _smtpOptions.Password, cancellationToken);
            await smtpClient.SendAsync(email, cancellationToken);
            await smtpClient.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
        }
    }
}
