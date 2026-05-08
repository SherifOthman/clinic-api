using ClinicManagement.Application.Abstractions.Email;

namespace ClinicManagement.Infrastructure.Services.Emails;

/// <summary>
/// Email address confirmation email sent after registration.
/// </summary>
public sealed class EmailConfirmationEmail : IEmail
{
    public string  ToEmail { get; }
    public string? ToName  { get; }
    public string  Subject => "Confirm your email address";
    public string  Body    { get; }
    public bool    IsHtml  => true;

    public EmailConfirmationEmail(string toEmail, string firstName, string confirmationLink)
    {
        ToEmail = toEmail;
        ToName  = firstName;
        Body    = EmailTemplates.GetEmailConfirmationTemplate(firstName, confirmationLink);
    }
}
