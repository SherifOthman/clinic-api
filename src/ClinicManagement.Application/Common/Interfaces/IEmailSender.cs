
using System.Net.Mail;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IEmailSender
{
    public Task SendEmailAsync(string toEmail, string subject, string htmlMessage, CancellationToken cancellationToken = default);
}
