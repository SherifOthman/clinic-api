namespace ClinicManagement.Application.Common.Interfaces;

/// <summary>
/// Service for generating and validating secure tokens for email confirmation and password reset
/// </summary>
public interface ITokenGenerator
{
    string GenerateEmailConfirmationToken(int userId, string email);
    bool ValidateEmailConfirmationToken(int userId, string email, string token);
    
    string GeneratePasswordResetToken(int userId, string email, string passwordHash);
    bool ValidatePasswordResetToken(int userId, string email, string passwordHash, string token);
}
