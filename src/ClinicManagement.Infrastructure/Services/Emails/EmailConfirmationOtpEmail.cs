using ClinicManagement.Application.Abstractions.Email;

namespace ClinicManagement.Infrastructure.Services.Emails;

/// <summary>
/// Email confirmation via 6-digit OTP — works on web and mobile without redirect links.
/// </summary>
public sealed class EmailConfirmationOtpEmail : IEmail
{
    public string  ToEmail { get; }
    public string? ToName  { get; }
    public string  Subject => "Your verification code";
    public string  Body    { get; }
    public bool    IsHtml  => true;

    public EmailConfirmationOtpEmail(string toEmail, string firstName, string otp)
    {
        ToEmail = toEmail;
        ToName  = firstName;
        Body    = EmailTemplates.GetEmailConfirmationOtpTemplate(firstName, otp);
    }
}
