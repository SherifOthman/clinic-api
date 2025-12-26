using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Options;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ClinicManagement.Infrastructure.Services;

internal class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;

    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage, CancellationToken cancellationToken = default)
    {
        try
        {
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
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
        }
    }
}
