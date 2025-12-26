using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Utils;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using System.Security.Claims;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Authentication service that encapsulates all authentication business logic.
/// Implements complete authentication flows following Clean Architecture principles.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly ITokenService _tokenService;
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;

    public AuthenticationService(
        ITokenService tokenService,
        IIdentityService identityService,
        IUnitOfWork unitOfWork)
    {
        _tokenService = tokenService;
        _identityService = identityService;
        _unitOfWork = unitOfWork;
    }

    public ClaimsPrincipal? ValidateAccessToken(string? accessToken)
    {
        return _tokenService.ValidateAccessToken(accessToken ?? string.Empty);
    }

    public async Task<Result<TokenRefreshResult>> RefreshTokenAsync(string? refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Result<TokenRefreshResult>.Fail("No refresh token provided");
        }

        // Validate refresh token
        var refreshTokenEntity = await _tokenService.ValidateRefreshTokenAsync(refreshToken, cancellationToken);
        if (refreshTokenEntity == null)
        {
            return Result<TokenRefreshResult>.Fail("Invalid or expired refresh token");
        }

        // Get user
        var user = await _unitOfWork.Users.GetByIdAsync(refreshTokenEntity.UserId, cancellationToken);
        if (user == null)
        {
            await _tokenService.RevokeRefreshTokenAsync(refreshToken, cancellationToken);
            return Result<TokenRefreshResult>.Fail("User not found");
        }

        // Get user roles and clinic ID
        var userRoles = await _identityService.GetUserRolesAsync(user, cancellationToken);
        var clinicId = await GetUserClinicIdAsync(user.Id, userRoles, cancellationToken);

        // Generate new tokens
        var newAccessToken = _tokenService.GenerateAccessToken(user, userRoles, clinicId);
        var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(user, cancellationToken);

        // Revoke old refresh token (security best practice)
        await _tokenService.RevokeRefreshTokenAsync(refreshToken, cancellationToken);

        // Create user principal for the new access token
        var userPrincipal = _tokenService.ValidateAccessToken(newAccessToken);
        if (userPrincipal == null)
        {
            return Result<TokenRefreshResult>.Fail("Failed to create user principal");
        }

        return Result<TokenRefreshResult>.Ok(new TokenRefreshResult
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            UserPrincipal = userPrincipal
        });
    }

    public async Task<Result<LoginResult>> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        // Validate credentials
        bool isEmail = StringUtils.IsEmail(email);
        var user = isEmail
            ? await _identityService.GetUserByEmailAsync(email, cancellationToken)
            : await _identityService.GetByUsernameAsync(email, cancellationToken);

        if (user == null || !await _identityService.CheckPasswordAsync(user, password, cancellationToken))
        {
            return Result<LoginResult>.Fail("Invalid username or password");
        }

        // Enforce email verification
        if (!await _identityService.IsEmailConfirmedAsync(user, cancellationToken))
        {
            return Result<LoginResult>.FailField("email", "Please verify your email address before signing in. Check your inbox for the verification link.");
        }

        // Get user roles and clinic ID
        var userRoles = await _identityService.GetUserRolesAsync(user, cancellationToken);
        var clinicId = await GetUserClinicIdAsync(user.Id, userRoles, cancellationToken);

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user, userRoles, clinicId);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user, cancellationToken);

        // Build user DTO
        var userDto = user.Adapt<UserDto>();
        userDto.Roles = userRoles.ToList();
        userDto.ClinicId = clinicId;

        return Result<LoginResult>.Ok(new LoginResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = userDto
        });
    }

    public async Task<Result<bool>> LogoutAsync(string? refreshToken, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _tokenService.RevokeRefreshTokenAsync(refreshToken, cancellationToken);
        }
        return Result<bool>.Ok(true);
    }

    private async Task<int?> GetUserClinicIdAsync(int userId, IEnumerable<string> roles, CancellationToken cancellationToken)
    {
        if (roles.Contains(UserRole.ClinicOwner.ToString()))
        {
            var clinic = await _unitOfWork.Clinics.GetByOwnerIdAsync(userId, cancellationToken);
            return clinic?.Id;
        }

        if (roles.Contains(UserRole.Doctor.ToString()))
        {
            var doctor = await _unitOfWork.Doctors.GetByUserIdAsync(userId, cancellationToken);
            return doctor?.ClinicId;
        }

        if (roles.Contains(UserRole.Receptionist.ToString()))
        {
            var receptionist = await _unitOfWork.Receptionists.GetByUserIdAsync(userId, cancellationToken);
            return receptionist?.ClinicId;
        }

        return null;
    }
}