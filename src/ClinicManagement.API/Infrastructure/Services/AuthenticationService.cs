using ClinicManagement.API.Common.Models;
using ClinicManagement.API.Common.Constants;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.API.Infrastructure.Services;

public class AuthenticationService
{
    private readonly TokenService _tokenService;
    private readonly UserManagementService _userManagementService;
    private readonly EmailConfirmationService _emailConfirmationService;
    private readonly RefreshTokenService _refreshTokenService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        TokenService tokenService,
        UserManagementService userManagementService,
        EmailConfirmationService emailConfirmationService,
        RefreshTokenService refreshTokenService,
        ILogger<AuthenticationService> logger)
    {
        _tokenService = tokenService;
        _userManagementService = userManagementService;
        _emailConfirmationService = emailConfirmationService;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    public async Task<TokenRefreshResult?> RefreshTokenAsync(string? refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("Refresh token attempt with empty token");
            return null;
        }

        var tokenEntity = await _refreshTokenService.GetActiveRefreshTokenAsync(refreshToken, cancellationToken);
        if (tokenEntity == null)
        {
            _logger.LogWarning("Invalid refresh token used");
            return null;
        }

        // Get user roles and clinic information
        var userRoles = await _userManagementService.GetUserRolesAsync(tokenEntity.User, cancellationToken);
        
        // Clinic ID is stored directly on the User entity
        var clinicId = tokenEntity.User.ClinicId;
        
        var newAccessToken = _tokenService.GenerateAccessToken(tokenEntity.User, userRoles, clinicId);
        var newRefreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(tokenEntity.UserId, null, cancellationToken);

        // Revoke old refresh token
        await _refreshTokenService.RevokeRefreshTokenAsync(refreshToken, null, newRefreshToken.Token, cancellationToken);

        _logger.LogInformation("Tokens refreshed for user {UserId}", tokenEntity.UserId);

        return new TokenRefreshResult(newAccessToken, newRefreshToken.Token);
    }

    public async Task<LoginResult?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Login attempt for user: {Email}", email);

        try
        {
            bool isEmail = email.Contains('@');
            var user = isEmail
                ? await _userManagementService.GetUserByEmailAsync(email, cancellationToken)
                : await _userManagementService.GetByUsernameAsync(email, cancellationToken);

            if (user == null || !await _userManagementService.CheckPasswordAsync(user, password, cancellationToken))
            {
                _logger.LogWarning("Failed login attempt for: {Email} - Invalid credentials", email);
                return null;
            }

            if (!await _emailConfirmationService.IsEmailConfirmedAsync(user, cancellationToken))
            {
                _logger.LogWarning("Login attempt with unconfirmed email: {Email}", email);
                return null;
            }

            // Get user roles and clinic information
            var userRoles = await _userManagementService.GetUserRolesAsync(user, cancellationToken);
            
            // Clinic ID is stored directly on the User entity
            var clinicId = user.ClinicId;

            // Generate tokens with clinic claims
            var accessToken = _tokenService.GenerateAccessToken(user, userRoles, clinicId);
            var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, null, cancellationToken);

            _logger.LogInformation("User {UserId} ({Email}) logged in successfully with roles: {Roles}, ClinicId: {ClinicId}", 
                user.Id, user.Email, string.Join(", ", userRoles), clinicId);

            return new LoginResult(accessToken, refreshToken.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login attempt for: {Email}", email);
            throw;
        }
    }

    public async Task LogoutAsync(string? refreshToken, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _refreshTokenService.RevokeRefreshTokenAsync(refreshToken, null, null, cancellationToken);
            _logger.LogInformation("User logged out and refresh token revoked");
        }
    }
}

public record TokenRefreshResult(string AccessToken, string RefreshToken);
public record LoginResult(string AccessToken, string RefreshToken);
