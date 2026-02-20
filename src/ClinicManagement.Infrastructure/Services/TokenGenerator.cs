using ClinicManagement.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.DataProtection;

namespace ClinicManagement.Infrastructure.Services;

public class TokenGenerator : ITokenGenerator
{
    private readonly IDataProtector _emailProtector;
    private readonly IDataProtector _passwordProtector;
    private readonly TimeSpan _tokenLifetime = TimeSpan.FromHours(24);

    public TokenGenerator(IDataProtectionProvider dataProtectionProvider)
    {
        _emailProtector = dataProtectionProvider.CreateProtector("EmailConfirmation");
        _passwordProtector = dataProtectionProvider.CreateProtector("PasswordReset");
    }

    public string GenerateEmailConfirmationToken(int userId, string email)
    {
        var payload = $"{userId}|{email}|{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        return _emailProtector.Protect(payload);
    }

    public bool ValidateEmailConfirmationToken(int userId, string email, string token)
    {
        try
        {
            var payload = _emailProtector.Unprotect(token);
            var parts = payload.Split('|');
            
            if (parts.Length != 3)
                return false;

            var tokenUserId = int.Parse(parts[0]);
            var tokenEmail = parts[1];
            var timestamp = long.Parse(parts[2]);
            
            var tokenTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            var isExpired = DateTimeOffset.UtcNow - tokenTime > _tokenLifetime;

            return tokenUserId == userId && 
                   tokenEmail.Equals(email, StringComparison.OrdinalIgnoreCase) && 
                   !isExpired;
        }
        catch
        {
            return false;
        }
    }

    public string GeneratePasswordResetToken(int userId, string email, string passwordHash)
    {
        var payload = $"{userId}|{email}|{passwordHash}|{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        return _passwordProtector.Protect(payload);
    }

    public bool ValidatePasswordResetToken(int userId, string email, string passwordHash, string token)
    {
        try
        {
            var payload = _passwordProtector.Unprotect(token);
            var parts = payload.Split('|');
            
            if (parts.Length != 4)
                return false;

            var tokenUserId = int.Parse(parts[0]);
            var tokenEmail = parts[1];
            var tokenPasswordHash = parts[2];
            var timestamp = long.Parse(parts[3]);
            
            var tokenTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            var isExpired = DateTimeOffset.UtcNow - tokenTime > _tokenLifetime;

            return tokenUserId == userId && 
                   tokenEmail.Equals(email, StringComparison.OrdinalIgnoreCase) && 
                   tokenPasswordHash == passwordHash &&
                   !isExpired;
        }
        catch
        {
            return false;
        }
    }
}

