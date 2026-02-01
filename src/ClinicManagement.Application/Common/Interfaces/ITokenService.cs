using ClinicManagement.Domain.Entities;
using System.Security.Claims;

namespace ClinicManagement.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles, Guid? clinicId = null);
    Task<string> GenerateRefreshTokenAsync(User user, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string refreshTokens, CancellationToken cancellationToken = default);
    
    ClaimsPrincipal? ValidateAccessToken(string token);
    
    bool IsTokenExpired(string token);

    (ClaimsPrincipal? principal, bool isExpired) ValidateAccessTokenWithExpiry(string token);
}
