using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using System.Security.Claims;

namespace ClinicManagement.Application.Common.Interfaces;

/// <summary>
/// Authentication service that handles complete authentication flows.
/// Encapsulates all authentication business logic in the Application layer.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Validates access token and returns user principal if valid.
    /// </summary>
    /// <param name="accessToken">Access token to validate</param>
    /// <returns>ClaimsPrincipal if valid, null if invalid</returns>
    ClaimsPrincipal? ValidateAccessToken(string? accessToken);

    /// <summary>
    /// Performs complete token refresh flow.
    /// Validates refresh token, generates new tokens, and returns user principal.
    /// </summary>
    /// <param name="refreshToken">Refresh token to use for refresh</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing new tokens and user principal</returns>
    Task<Result<TokenRefreshResult>> RefreshTokenAsync(string? refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs complete login flow.
    /// Validates credentials, generates tokens, and returns user data.
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="password">User password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing tokens and user data</returns>
    Task<Result<LoginResult>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears user authentication (revokes refresh token).
    /// </summary>
    /// <param name="refreshToken">Refresh token to revoke</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success</returns>
    Task<Result<bool>> LogoutAsync(string? refreshToken, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of token refresh operation
/// </summary>
public class TokenRefreshResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public ClaimsPrincipal UserPrincipal { get; set; } = null!;
}

/// <summary>
/// Result of login operation
/// </summary>
public class LoginResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}