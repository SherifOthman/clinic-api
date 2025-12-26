using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ClinicManagement.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles, int? clinicId = null);
    Task<string> GenerateRefreshTokenAsync(User user, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string refreshTokens, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates an access token and returns the ClaimsPrincipal if valid.
    /// Returns null if the token is invalid or expired.
    /// </summary>
    /// <param name="token">The JWT access token to validate</param>
    /// <returns>ClaimsPrincipal if valid, null otherwise</returns>
    ClaimsPrincipal? ValidateAccessToken(string token);
    
    /// <summary>
    /// Validates a refresh token and returns the RefreshToken entity if valid.
    /// Returns null if the token is invalid, expired, or revoked.
    /// </summary>
    /// <param name="token">The refresh token to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>RefreshToken entity if valid, null otherwise</returns>
    Task<RefreshToken?> ValidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
}
