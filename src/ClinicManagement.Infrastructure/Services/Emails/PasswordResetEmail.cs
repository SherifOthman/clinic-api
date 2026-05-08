using ClinicManagement.Application.Abstractions.Email;

namespace ClinicManagement.Infrastructure.Services.Emails;

/// <summary>
/// Password reset email. Adding a new email type = new class here, not a new
/// method on EmailService (OCP).
/// </summary>
public sealed class PasswordResetEmail : IEmail
{
    public string  ToEmail { get; }
    public string? ToName  { get; }
    public string  Subject => "Reset your password";
    public string  Body    { get; }
    public bool    IsHtml  => true;

    public PasswordResetEmail(string toEmail, string userName, string resetLink)
    {
        ToEmail = toEmail;
        ToName  = userName;
        Body    = EmailTemplates.GetPasswordResetTemplate(userName, resetLink);
    }
}
