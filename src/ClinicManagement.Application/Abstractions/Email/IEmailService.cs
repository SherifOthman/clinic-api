namespace ClinicManagement.Application.Abstractions.Email;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink, CancellationToken cancellationToken = default);
    Task SendStaffInvitationEmailAsync(string toEmail, string clinicName, string role, string invitedBy, string invitationLink, CancellationToken cancellationToken = default);
}
