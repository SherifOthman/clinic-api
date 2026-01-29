using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.Common.Constants;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ClinicManagement.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly ITokenService _tokenService;
    private readonly IUserManagementService _userManagementService;
    private readonly IEmailConfirmationService _emailConfirmationService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        ITokenService tokenService,
        IUserManagementService userManagementService,
        IEmailConfirmationService emailConfirmationService,
        IRefreshTokenService refreshTokenService,
        ILogger<AuthenticationService> logger)
    {
        _tokenService = tokenService;
        _userManagementService = userManagementService;
        _emailConfirmationService = emailConfirmationService;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    public ClaimsPrincipal? ValidateAccessToken(string? accessToken)
    {
        return _tokenService.ValidateAccessToken(accessToken ?? string.Empty);
    }

    public async Task<Result<TokenRefreshResult>> RefreshTokenAsync(string? refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("Refresh token attempt with empty token");
            return Result<TokenRefreshResult>.Fail(MessageCodes.Authentication.INVALID_RESET_TOKEN);
        }

        var tokenEntity = await _refreshTokenService.GetActiveRefreshTokenAsync(refreshToken, cancellationToken);
        if (tokenEntity == null)
        {
            _logger.LogWarning("Invalid refresh token used");
            return Result<TokenRefreshResult>.Fail(MessageCodes.Authentication.INVALID_RESET_TOKEN);
        }

        // Generate new tokens
        var userRoles = await _userManagementService.GetUserRolesAsync(tokenEntity.User, cancellationToken);
        var newAccessToken = _tokenService.GenerateAccessToken(tokenEntity.User, userRoles, tokenEntity.User.ClinicId);
        var newRefreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(tokenEntity.UserId, null, cancellationToken);

        // Revoke old refresh token
        await _refreshTokenService.RevokeRefreshTokenAsync(refreshToken, null, newRefreshToken.Token, cancellationToken);

        _logger.LogInformation("Tokens refreshed for user {UserId}", tokenEntity.UserId);

        return Result<TokenRefreshResult>.Ok(new TokenRefreshResult
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token
        });
    }

    public async Task<Result<LoginResult>> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        bool isEmail = email.Contains('@');
        var user = isEmail
            ? await _userManagementService.GetUserByEmailAsync(email, cancellationToken)
            : await _userManagementService.GetByUsernameAsync(email, cancellationToken);

        if (user == null || !await _userManagementService.CheckPasswordAsync(user, password, cancellationToken))
        {
            _logger.LogWarning("Failed login attempt for: {Email}", email);
            return Result<LoginResult>.Fail(MessageCodes.Authentication.INVALID_CREDENTIALS);
        }

        if (!await _emailConfirmationService.IsEmailConfirmedAsync(user, cancellationToken))
        {
            _logger.LogWarning("Login attempt with unconfirmed email: {Email}", email);
            return Result<LoginResult>.Fail(MessageCodes.Authentication.EMAIL_NOT_CONFIRMED);
        }

        var userRoles = await _userManagementService.GetUserRolesAsync(user, cancellationToken);
        var accessToken = _tokenService.GenerateAccessToken(user, userRoles, user.ClinicId);
        var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, null, cancellationToken);

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return Result<LoginResult>.Ok(new LoginResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token
        });
    }

    public async Task<Result<bool>> LogoutAsync(string? refreshToken, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _refreshTokenService.RevokeRefreshTokenAsync(refreshToken, null, null, cancellationToken);
            _logger.LogInformation("User logged out and refresh token revoked");
        }
        
        return Result<bool>.Ok(true);
    }
}