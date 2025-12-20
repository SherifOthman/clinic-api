using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Options;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ClinicManagement.Infrastructure.Services;

internal class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending email to {ToEmail} with subject: {Subject}", toEmail, subject);

            // Replace frontend URL placeholder in email template
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

            using var smtp = new SmtpClient();
            
            // Connect to SMTP server
            await smtp.ConnectAsync(_options.Host, _options.Port,
                MailKit.Security.SecureSocketOptions.StartTls, cancellationToken);
            
            // Authenticate
            await smtp.AuthenticateAsync(_options.UserName, _options.Password, cancellationToken);
            
            // Send email
            await smtp.SendAsync(email, cancellationToken);
            
            // Disconnect
            await smtp.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail}. Error: {Error}", toEmail, ex.Message);
            throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
        }
    }
}
