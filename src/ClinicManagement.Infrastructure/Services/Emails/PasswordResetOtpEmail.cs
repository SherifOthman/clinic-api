using ClinicManagement.Application.Abstractions.Email;

namespace ClinicManagement.Infrastructure.Services.Emails;

/// <summary>
/// Password reset via 6-digit OTP — works on web and mobile without redirect links.
/// </summary>
public sealed class PasswordResetOtpEmail : IEmail
{
    public string  ToEmail { get; }
    public string? ToName  { get; }
    public string  Subject => "Your password reset code";
    public string  Body    { get; }
    public bool    IsHtml  => true;

    public PasswordResetOtpEmail(string toEmail, string firstName, string otp)
    {
        ToEmail = toEmail;
        ToName  = firstName;
        Body    = EmailTemplates.GetPasswordResetOtpTemplate(firstName, otp);
    }
}
