using ClinicManagement.Application.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ClinicManagement.Infrastructure.Services;

public class SmtpEmailSender
{
    private readonly SmtpOptions _options;
    private readonly MailKitSmtpClient _smtpClient;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> options, MailKitSmtpClient smtpClient, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _smtpClient = smtpClient;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending email to {Email} with subject: {Subject}", toEmail, subject);

            htmlMessage = htmlMessage.Replace("{{FRONTEND_URL}}", _options.FrontendUrl);

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };
            email.Body = bodyBuilder.ToMessageBody();

            await _smtpClient.ConnectAsync(_options.Host, _options.Port,
                MailKit.Security.SecureSocketOptions.StartTls, cancellationToken);
            await _smtpClient.AuthenticateAsync(_options.UserName, _options.Password, cancellationToken);
            await _smtpClient.SendAsync(email, cancellationToken);
            await _smtpClient.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
        }
    }
}
