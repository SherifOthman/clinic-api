using ClinicManagement.Application.Abstractions.Email;

namespace ClinicManagement.Infrastructure.Services.Emails;

/// <summary>
/// Staff invitation email sent when a clinic owner invites a new team member.
/// </summary>
public sealed class StaffInvitationEmail : IEmail
{
    public string  ToEmail  { get; }
    public string? ToName   => null;
    public string  Subject  { get; }
    public string  Body     { get; }
    public bool    IsHtml   => true;

    public StaffInvitationEmail(
        string toEmail,
        string clinicName,
        string role,
        string invitedBy,
        string fullInvitationLink)
    {
        ToEmail = toEmail;
        Subject = $"Invitation to join {clinicName} as {role}";
        Body    = EmailTemplates.GetStaffInvitationTemplate(clinicName, role, invitedBy, fullInvitationLink);
    }
}
