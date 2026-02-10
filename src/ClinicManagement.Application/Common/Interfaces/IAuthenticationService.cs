using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using System.Security.Claims;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IAuthenticationService
{
    Task<Result<TokenRefreshResult>> RefreshTokenAsync(string? refreshToken, CancellationToken cancellationToken = default);

    Task<Result<LoginResult>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<Result<bool>> LogoutAsync(string? refreshToken, CancellationToken cancellationToken = default);
}

public class TokenRefreshResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public ClaimsPrincipal UserPrincipal { get; set; } = null!;
}

public class LoginResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
