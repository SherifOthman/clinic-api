using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Abstractions.Storage;
using Microsoft.Extensions.Configuration;

namespace ClinicManagement.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SmtpEmailSender _smtpEmailSender;
    private readonly IConfiguration _configuration;

    public EmailService(SmtpEmailSender smtpEmailSender, IConfiguration configuration)
    {
        _smtpEmailSender = smtpEmailSender;
        _configuration = configuration;
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

    public async Task SendStaffInvitationEmailAsync(
        string toEmail,
        string clinicName,
        string role,
        string invitedBy,
        string invitationLink,
        CancellationToken cancellationToken = default)
    {
        var frontendUrl = _configuration["Email:FrontendUrl"] ?? "http://localhost:3000";
        var fullInvitationLink = invitationLink.StartsWith("http") 
            ? invitationLink 
            : $"{frontendUrl}{invitationLink}";
            
        var emailBody = EmailTemplates.GetStaffInvitationTemplate(clinicName, role, invitedBy, fullInvitationLink);
        await _smtpEmailSender.SendEmailAsync(
            toEmail,
            $"Invitation to join {clinicName} as {role}",
            emailBody,
            cancellationToken);
    }
}

