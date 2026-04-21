using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, Result<TokenResponseDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISecurityAuditWriter _auditWriter;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IUnitOfWork uow,
        UserManager<User> userManager,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService,
        ICurrentUserService currentUserService,
        ISecurityAuditWriter auditWriter,
        ILogger<LoginHandler> logger)
    {
        _uow                = uow;
        _userManager        = userManager;
        _tokenService       = tokenService;
        _refreshTokenService = refreshTokenService;
        _currentUserService = currentUserService;
        _auditWriter        = auditWriter;
        _logger             = logger;
    }

    public async Task<Result<TokenResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByEmailOrUsernameAsync(request.EmailOrUsername, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Failed login attempt for {EmailOrUsername} - user not found", request.EmailOrUsername);
            await _auditWriter.WriteAsync(null, null, null, null, null, null,
                "LoginFailed", $"User not found: {request.EmailOrUsername}", cancellationToken);
            return Result.Failure<TokenResponseDto>(ErrorCodes.INVALID_CREDENTIALS, "Invalid email/username or password");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            var lockoutEnd        = await _userManager.GetLockoutEndDateAsync(user);
            var remainingMinutes  = lockoutEnd.HasValue ? (int)(lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes + 1 : 30;

            _logger.LogWarning("Login attempt for locked account {UserId}", user.Id);
            await _auditWriter.WriteAsync(user.Id, user.FullName,
                user.UserName, user.Email, null, null,
                "LoginBlocked", $"Account locked, {remainingMinutes} min remaining", cancellationToken);

            return Result.Failure<TokenResponseDto>(ErrorCodes.ACCOUNT_LOCKED,
                $"Account is locked due to multiple failed login attempts. Please try again in {remainingMinutes} minute(s).");
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            _logger.LogWarning("Failed login attempt for {EmailOrUsername} - invalid password", request.EmailOrUsername);
            await _userManager.AccessFailedAsync(user);

            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Account {UserId} locked after multiple failed login attempts", user.Id);
                await _auditWriter.WriteAsync(user.Id, user.FullName,
                    user.UserName, user.Email, null, null,
                    "AccountLocked", "Locked after too many failed attempts", cancellationToken);
                return Result.Failure<TokenResponseDto>(ErrorCodes.ACCOUNT_LOCKED,
                    "Account is locked due to multiple failed login attempts. Please try again in 30 minutes.");
            }

            await _auditWriter.WriteAsync(user.Id, user.FullName,
                user.UserName, user.Email, null, null, "LoginFailed", "Invalid password", cancellationToken);
            return Result.Failure<TokenResponseDto>(ErrorCodes.INVALID_CREDENTIALS, "Invalid email/username or password");
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _uow.SaveChangesAsync(cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);

        Guid? clinicId = null;
        string? countryCode = null;
        if (roles.Contains(UserRoles.ClinicOwner))
        {
            var clinic = await _uow.Clinics.GetByOwnerIdAsync(user.Id, cancellationToken);
            clinicId    = clinic?.Id;
            countryCode = clinic?.CountryCode;
        }
        else
        {
            var staff = await _uow.Members.GetByUserIdIgnoreFiltersAsync(user.Id, cancellationToken);
            clinicId  = staff?.ClinicId;

            if (staff is not null && !staff.IsActive)
            {
                _logger.LogWarning("Inactive staff {UserId} attempted to log in", user.Id);
                await _auditWriter.WriteAsync(user.Id, user.FullName,
                    user.UserName, user.Email, null, clinicId, "LoginFailed", "Account inactive", cancellationToken);
                return Result.Failure<TokenResponseDto>(ErrorCodes.STAFF_INACTIVE,
                    "Your account has been deactivated. Please contact your clinic owner.");
            }

            if (clinicId.HasValue)
            {
                var clinic = await _uow.Clinics.GetByIdAsync(clinicId.Value, cancellationToken);
                countryCode = clinic?.CountryCode;
            }
        }

        var accessToken  = _tokenService.GenerateAccessToken(user, roles.ToList(), 
            await LoadPermissionsAsync(user.Id, roles, clinicId, cancellationToken),
            clinicId, countryCode);
        var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, null, cancellationToken);

        await _auditWriter.WriteAsync(user.Id, user.FullName,
            user.UserName, user.Email, string.Join(",", roles), clinicId,
            "LoginSuccess", cancellationToken: cancellationToken);

        _logger.LogInformation("User {UserId} logged in ({ClientType}) with roles [{Roles}]",
            user.Id, request.IsMobile ? "mobile" : "web", string.Join(", ", roles));

        return Result.Success(new TokenResponseDto(accessToken, refreshToken.Token));
    }

    private async Task<List<Permission>> LoadPermissionsAsync(
        Guid userId, IList<string> roles, Guid? clinicId, CancellationToken ct)
    {
        if (clinicId is null) return [];
        var member = await _uow.Members.GetByUserIdIgnoreFiltersAsync(userId, ct);
        if (member is null) return [];
        return await _uow.Permissions.GetByMemberIdAsync(member.Id, ct);
    }
}
