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
        // ── 1. Load user ──────────────────────────────────────────────────────
        var user = await _uow.Users.GetByEmailOrUsernameAsync(request.EmailOrUsername, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Login failed — user not found: {Input}", request.EmailOrUsername);
            await _audit.WriteEventAsync("LoginFailed", $"User not found: {request.EmailOrUsername}", ct: cancellationToken);
            return Result.Failure<TokenResponseDto>(ErrorCodes.INVALID_CREDENTIALS, "Invalid email/username or password");
        }

        // ── 2. Lockout check ──────────────────────────────────────────────────
        if (await _userManager.IsLockedOutAsync(user))
        {
            var lockoutEnd       = await _userManager.GetLockoutEndDateAsync(user);
            var remainingMinutes = lockoutEnd.HasValue
                ? (int)(lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes + 1
                : 30;

            _logger.LogWarning("Login blocked — account locked: {UserId}", user.Id);
            await _audit.WriteEventAsync("LoginBlocked", $"Account locked, {remainingMinutes} min remaining",
                overrideUserId: user.Id, overrideFullName: user.Person.FullName,
                overrideEmail: user.Email, ct: cancellationToken);

            return Result.Failure<TokenResponseDto>(ErrorCodes.ACCOUNT_LOCKED,
                $"Account is locked. Please try again in {remainingMinutes} minute(s).");
        }

        // ── 3. Password check ─────────────────────────────────────────────────
        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            _logger.LogWarning("Login failed — wrong password: {UserId}", user.Id);
            await _userManager.AccessFailedAsync(user);

            // Re-check: did this attempt trigger a lockout?
            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Account locked after failed attempts: {UserId}", user.Id);
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

        // ── 4. Stamp last login and reset fail count ──────────────────────────
        await _userManager.ResetAccessFailedCountAsync(user);
        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _uow.SaveChangesAsync(cancellationToken);

        // ── 5. Resolve clinic/member context for the JWT ──────────────────────
        var roles = (await _userManager.GetRolesAsync(user)).ToList();

        var contextResult = await ResolveLoginContextAsync(user.Id, roles, cancellationToken);
        if (contextResult.IsFailure)
        {
            await _audit.WriteEventAsync("LoginFailed", contextResult.ErrorMessage,
                overrideUserId: user.Id, overrideFullName: user.Person.FullName,
                overrideEmail: user.Email, ct: cancellationToken);

            return Result.Failure<TokenResponseDto>(contextResult.ErrorCode!, contextResult.ErrorMessage!);
        }

        var (clinicId, memberId, countryCode) = contextResult.Value!;

        // ── 6. Issue tokens ───────────────────────────────────────────────────
        var accessToken  = _tokenService.GenerateAccessToken(user, roles, memberId, clinicId, countryCode);
        var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, null, cancellationToken);

        // ── 7. Audit success ──────────────────────────────────────────────────
        await _audit.WriteEventAsync("LoginSuccess",
            overrideUserId: user.Id, overrideFullName: user.Person.FullName,
            overrideEmail: user.Email, overrideRole: string.Join(",", roles),
            overrideClinicId: clinicId, ct: cancellationToken);

        _logger.LogInformation("User {UserId} logged in ({ClientType}) [{Roles}]",
            user.Id, request.IsMobile ? "mobile" : "web", string.Join(", ", roles));

        return Result.Success(new TokenResponseDto(accessToken, refreshToken.Token));
    }

    // ── Context resolution ────────────────────────────────────────────────────

    /// <summary>
    /// Resolves the clinic, member, and country context needed to build the JWT.
    ///
    /// Three cases:
    ///   ClinicOwner — owns a clinic; may also have a member record (if registered as doctor).
    ///   Staff       — belongs to a clinic as Doctor/Receptionist/Nurse.
    ///   SuperAdmin  — no clinic, no member; clinicId and memberId are null.
    /// </summary>
    private async Task<Result<LoginContext>> ResolveLoginContextAsync(
        Guid userId, List<string> roles, CancellationToken ct)
    {
        if (roles.Contains(UserRoles.ClinicOwner))
        {
            var clinic = await _uow.Clinics.GetByOwnerIdAsync(userId, ct);

            // Owner may also be a doctor — look up their member record
            Guid? memberId = null;
            if (clinic is not null)
            {
                var member = await _uow.Members.GetByUserIdIgnoreFiltersAsync(userId, ct);
                memberId = member?.Id;
            }

            return Result.Success(new LoginContext(clinic?.Id, memberId, clinic?.CountryCode));
        }

        if (roles.Contains(UserRoles.SuperAdmin))
        {
            // SuperAdmin has no clinic or member record
            return Result.Success(new LoginContext(null, null, null));
        }

        // Staff: Doctor, Receptionist, Nurse
        var staff = await _uow.Members.GetByUserIdIgnoreFiltersAsync(userId, ct);
        if (staff is null)
            return Result.Success(new LoginContext(null, null, null));

        if (!staff.IsActive)
            return Result.Failure<LoginContext>(ErrorCodes.STAFF_INACTIVE,
                "Your account has been deactivated. Please contact your clinic owner.");

        // Get countryCode without loading the full Clinic entity
        var countryCode = await _uow.Clinics.GetCountryCodeAsync(staff.ClinicId, ct);

        return Result.Success(new LoginContext(staff.ClinicId, staff.Id, countryCode));
    }

    private record LoginContext(Guid? ClinicId, Guid? MemberId, string? CountryCode);
}
