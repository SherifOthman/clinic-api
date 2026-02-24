using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Email;

/// <summary>
/// Service for email-related token operations and email confirmation workflow
/// </summary>
public interface IEmailTokenService
{
    // Email Confirmation
    Task SendConfirmationEmailAsync(User user, CancellationToken cancellationToken = default);
    Task ConfirmEmailAsync(User user, string token, CancellationToken cancellationToken = default);
    Task<bool> IsEmailConfirmedAsync(User user, CancellationToken cancellationToken = default);
    
    // Password Reset Tokens
    string GeneratePasswordResetToken(Guid userId, string email, string passwordHash);
    bool ValidatePasswordResetToken(Guid userId, string email, string passwordHash, string token);
}
