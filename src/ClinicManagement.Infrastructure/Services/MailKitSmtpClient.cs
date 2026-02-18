using MailKit.Net.Smtp;
using MimeKit;

namespace ClinicManagement.Infrastructure.Services;

public class MailKitSmtpClient
{
    private readonly SmtpClient _smtpClient;

    public MailKitSmtpClient()
    {
        _smtpClient = new SmtpClient();
    }

    public async Task ConnectAsync(string host, int port, MailKit.Security.SecureSocketOptions options, CancellationToken cancellationToken = default)
    {
        await _smtpClient.ConnectAsync(host, port, options, cancellationToken);
    }

    public async Task AuthenticateAsync(string userName, string password, CancellationToken cancellationToken = default)
    {
        await _smtpClient.AuthenticateAsync(userName, password, cancellationToken);
    }

    public async Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default)
    {
        await _smtpClient.SendAsync(message, cancellationToken);
    }

    public async Task DisconnectAsync(bool quit, CancellationToken cancellationToken = default)
    {
        await _smtpClient.DisconnectAsync(quit, cancellationToken);
    }

    public void Dispose()
    {
        _smtpClient?.Dispose();
    }
}
