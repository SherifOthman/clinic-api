using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Email;

/// <summary>
/// Service for email confirmation workflow.
/// Password reset tokens are handled directly by UserManager in ForgotPasswordHandler
/// and ResetPasswordHandler — no abstraction needed there.
/// </summary>
public interface IEmailTokenService
{
    Task SendConfirmationEmailAsync(User user, CancellationToken cancellationToken = default);
    Task ConfirmEmailAsync(User user, string token, CancellationToken cancellationToken = default);
    Task<bool> IsEmailConfirmedAsync(User user, CancellationToken cancellationToken = default);
}
