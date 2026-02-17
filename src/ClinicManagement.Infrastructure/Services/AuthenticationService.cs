using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class AuthenticationService
{
    private readonly TokenService _tokenService;
    private readonly UserManager<User> _userManager;
    private readonly EmailConfirmationService _emailConfirmationService;
    private readonly RefreshTokenService _refreshTokenService;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        TokenService tokenService,
        UserManager<User> userManager,
        EmailConfirmationService emailConfirmationService,
        RefreshTokenService refreshTokenService,
        ApplicationDbContext db,
        ILogger<AuthenticationService> logger)
    {
        _tokenService = tokenService;
        _userManager = userManager;
        _emailConfirmationService = emailConfirmationService;
        _refreshTokenService = refreshTokenService;
        _db = db;
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

        // Get user roles
        var userRoles = await _userManager.GetRolesAsync(tokenEntity.User);
        
        // Get ClinicId from Staff table (if user is clinic staff)
        var staff = await _db.Staff
            .Where(s => s.UserId == tokenEntity.UserId && s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);
        
        var clinicId = staff?.ClinicId;
        
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
                ? await _userManager.FindByEmailAsync(email)
                : await _userManager.FindByNameAsync(email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            {
                _logger.LogWarning("Failed login attempt for: {Email} - Invalid credentials", email);
                return null;
            }

            // Note: We allow login even if email is not confirmed
            // Frontend will handle redirecting to email verification page
            if (!await _emailConfirmationService.IsEmailConfirmedAsync(user, cancellationToken))
            {
                _logger.LogInformation("User logged in with unconfirmed email: {Email}", email);
            }

            // Get user roles and clinic information
            var userRoles = await _userManager.GetRolesAsync(user);
            
            // Get ClinicId from Staff table (if user is clinic staff)
            var staff = await _db.Staff
                .Where(s => s.UserId == user.Id && s.IsActive)
                .FirstOrDefaultAsync(cancellationToken);
            
            var clinicId = staff?.ClinicId;

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
