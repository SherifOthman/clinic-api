using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
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
    private readonly IAuditWriter _audit;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IUnitOfWork uow,
        UserManager<User> userManager,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService,
        IAuditWriter audit,
        ILogger<LoginHandler> logger)
    {
        _uow                 = uow;
        _userManager         = userManager;
        _tokenService        = tokenService;
        _refreshTokenService = refreshTokenService;
        _audit               = audit;
        _logger              = logger;
    }

    public async Task<Result<TokenResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByEmailOrUsernameAsync(request.EmailOrUsername, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Failed login attempt for {EmailOrUsername} - user not found", request.EmailOrUsername);
            await _audit.WriteEventAsync("LoginFailed", $"User not found: {request.EmailOrUsername}", ct: cancellationToken);
            return Result.Failure<TokenResponseDto>(ErrorCodes.INVALID_CREDENTIALS, "Invalid email/username or password");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            var lockoutEnd       = await _userManager.GetLockoutEndDateAsync(user);
            var remainingMinutes = lockoutEnd.HasValue ? (int)(lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes + 1 : 30;

            _logger.LogWarning("Login attempt for locked account {UserId}", user.Id);
           
            await _audit.WriteEventAsync("LoginBlocked", $"Account locked, {remainingMinutes} min remaining",
                overrideUserId: user.Id, overrideFullName: user.Person.FullName,
                overrideEmail: user.Email, ct: cancellationToken);

            return Result.Failure<TokenResponseDto>(ErrorCodes.ACCOUNT_LOCKED,
                $"Account is locked. Please try again in {remainingMinutes} minute(s).");
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            _logger.LogWarning("Failed login attempt for {EmailOrUsername} - invalid password", request.EmailOrUsername);
            await _userManager.AccessFailedAsync(user);

            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Account {UserId} locked after multiple failed login attempts", user.Id);
              
                await _audit.WriteEventAsync("AccountLocked", "Locked after too many failed attempts",
                    overrideUserId: user.Id, overrideFullName: user.Person.FullName,
                    overrideEmail: user.Email, ct: cancellationToken);

                return Result.Failure<TokenResponseDto>(ErrorCodes.ACCOUNT_LOCKED,
                    "Account is locked due to multiple failed login attempts. Please try again in 30 minutes.");
            }

            await _audit.WriteEventAsync("LoginFailed", "Invalid password",
                overrideUserId: user.Id, overrideFullName: user.Person.FullName,
                overrideEmail: user.Email, ct: cancellationToken);

            return Result.Failure<TokenResponseDto>(ErrorCodes.INVALID_CREDENTIALS, "Invalid email/username or password");
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _uow.SaveChangesAsync(cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);

        Guid? clinicId    = null;
        Guid? memberId    = null;
        string? countryCode = null;

        if (roles.Contains(UserRoles.ClinicOwner))
        {
            var clinic  = await _uow.Clinics.GetByOwnerIdAsync(user.Id, cancellationToken);
            clinicId    = clinic?.Id;
            countryCode = clinic?.CountryCode;

            // Owner may also be registered as a doctor — look up their member record once
            if (clinicId.HasValue)
            {
                var ownerMember = await _uow.Members.GetByUserIdIgnoreFiltersAsync(user.Id, cancellationToken);
                memberId = ownerMember?.Id;
            }
        }
        else
        {
            var staff = await _uow.Members.GetByUserIdIgnoreFiltersAsync(user.Id, cancellationToken);
            clinicId  = staff?.ClinicId;
            memberId  = staff?.Id;  // already loaded — no second query needed

            if (staff is not null && !staff.IsActive)
            {
                _logger.LogWarning("Inactive staff {UserId} attempted to log in", user.Id);
                await _audit.WriteEventAsync("LoginFailed", "Account inactive",
                    overrideUserId: user.Id, overrideFullName: user.Person.FullName,
                    overrideEmail: user.Email, overrideClinicId: clinicId, ct: cancellationToken);

                return Result.Failure<TokenResponseDto>(ErrorCodes.STAFF_INACTIVE,
                    "Your account has been deactivated. Please contact your clinic owner.");
            }

            if (clinicId.HasValue)
            {
                var clinic  = await _uow.Clinics.GetByIdAsync(clinicId.Value, cancellationToken);
                countryCode = clinic?.CountryCode;
            }
        }

        var accessToken  = _tokenService.GenerateAccessToken(user, roles.ToList(), memberId, clinicId, countryCode);
        var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, null, cancellationToken);

        await _audit.WriteEventAsync("LoginSuccess",
            overrideUserId: user.Id, overrideFullName: user.Person.FullName,
            overrideEmail: user.Email, overrideRole: string.Join(",", roles),
            overrideClinicId: clinicId, ct: cancellationToken);

        _logger.LogInformation("User {UserId} logged in ({ClientType}) with roles [{Roles}]",
            user.Id, request.IsMobile ? "mobile" : "web", string.Join(", ", roles));

        return Result.Success(new TokenResponseDto(accessToken, refreshToken.Token));
    }
}
