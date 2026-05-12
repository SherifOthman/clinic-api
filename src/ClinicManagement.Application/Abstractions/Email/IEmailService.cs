namespace ClinicManagement.Application.Abstractions.Email;

public interface IEmailService
{
    /// <summary>
    /// Send a fully-formed email object.
    /// This is the only entry point — all email types implement IEmail.
    /// Adding a new email = new IEmail class, not a new method here (OCP).
    /// </summary>
    Task SendAsync(IEmail email, CancellationToken cancellationToken = default);

    /// <summary>Staff invitation still uses a link — kept as a named method for clarity.</summary>
    Task SendStaffInvitationEmailAsync(
        string toEmail, string clinicName, string role, string invitedBy,
        string invitationLink, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generic send for the email queue processor — sends pre-built subject/body from the queue.
    /// All auth emails use SendAsync(IEmail) instead.
    /// </summary>
    Task SendEmailAsync(
        string toEmail, string? toName, string subject, string body,
        bool isHtml = true, CancellationToken cancellationToken = default);
}
