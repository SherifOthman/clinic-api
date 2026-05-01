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

    // ── Entry point ───────────────────────────────────────────────────────────

    public async Task<Result<TokenResponseDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var userResult = await AuthenticateAsync(request, ct);
        if (userResult.IsFailure)
            return Result.Failure<TokenResponseDto>(userResult.ErrorCode!, userResult.ErrorMessage!);

        var user  = userResult.Value!;
        var roles = (await _userManager.GetRolesAsync(user)).ToList();

        var contextResult = await ResolveClinicContextAsync(user.Id, roles, ct);
        if (contextResult.IsFailure)
        {
            await AuditAsync("LoginFailed", contextResult.ErrorMessage, user, clinicId: null, ct);
            return Result.Failure<TokenResponseDto>(contextResult.ErrorCode!, contextResult.ErrorMessage!);
        }

        var ctx          = contextResult.Value!;
        var accessToken  = _tokenService.GenerateAccessToken(user, roles, ctx.MemberId, ctx.ClinicId, ctx.CountryCode);
        var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, null, ct);

        await AuditAsync("LoginSuccess", null, user, ctx.ClinicId, ct, roles);
        _logger.LogInformation("User {UserId} logged in ({Mode}) [{Roles}]",
            user.Id, request.IsMobile ? "mobile" : "web", string.Join(", ", roles));

        return Result.Success(new TokenResponseDto(accessToken, refreshToken.Token));
    }

    // ── Authentication ────────────────────────────────────────────────────────

    /// <summary>
    /// Validates the credentials and returns the authenticated user.
    /// Handles: user not found, account locked, wrong password, lockout-on-fail.
    /// Stamps LastLoginAt and resets the fail counter on success.
    /// </summary>
    private async Task<Result<User>> AuthenticateAsync(LoginCommand request, CancellationToken ct)
    {
        var user = await _uow.Users.GetByEmailOrUsernameAsync(request.EmailOrUsername, ct);
        if (user is null)
        {
            _logger.LogWarning("Login failed — user not found: {Input}", request.EmailOrUsername);
            await _audit.WriteEventAsync("LoginFailed", $"User not found: {request.EmailOrUsername}", ct: ct);
            return Result.Failure<User>(ErrorCodes.INVALID_CREDENTIALS, "Invalid email/username or password");
        }

        if (await _userManager.IsLockedOutAsync(user))
            return await FailLockedOutAsync(user, ct);

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            return await FailWrongPasswordAsync(user, ct);

        await _userManager.ResetAccessFailedCountAsync(user);
        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _uow.SaveChangesAsync(ct);

        return Result.Success(user);
    }

    private async Task<Result<User>> FailLockedOutAsync(User user, CancellationToken ct)
    {
        var lockoutEnd       = await _userManager.GetLockoutEndDateAsync(user);
        var remainingMinutes = lockoutEnd.HasValue
            ? (int)(lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes + 1
            : 30;

        _logger.LogWarning("Login blocked — account locked: {UserId}", user.Id);
        await AuditAsync("LoginBlocked", $"Account locked, {remainingMinutes} min remaining", user, clinicId: null, ct);

        return Result.Failure<User>(ErrorCodes.ACCOUNT_LOCKED,
            $"Account is locked. Please try again in {remainingMinutes} minute(s).");
    }

    private async Task<Result<User>> FailWrongPasswordAsync(User user, CancellationToken ct)
    {
        _logger.LogWarning("Login failed — wrong password: {UserId}", user.Id);
        await _userManager.AccessFailedAsync(user);

        if (await _userManager.IsLockedOutAsync(user))
        {
            _logger.LogWarning("Account locked after failed attempts: {UserId}", user.Id);
            await AuditAsync("AccountLocked", "Locked after too many failed attempts", user, clinicId: null, ct);
            return Result.Failure<User>(ErrorCodes.ACCOUNT_LOCKED,
                "Account is locked due to multiple failed login attempts. Please try again in 30 minutes.");
        }

        await AuditAsync("LoginFailed", "Invalid password", user, clinicId: null, ct);
        return Result.Failure<User>(ErrorCodes.INVALID_CREDENTIALS, "Invalid email/username or password");
    }

    // ── Clinic context ────────────────────────────────────────────────────────

    /// <summary>
    /// Resolves the clinic, member, and country context needed to build the JWT.
    /// Three cases:
    ///   ClinicOwner — owns a clinic; may also have a member record (if registered as doctor).
    ///   Staff       — belongs to a clinic as Doctor/Receptionist/Nurse.
    ///   SuperAdmin  — no clinic, no member; all context fields are null.
    /// </summary>
    private async Task<Result<LoginContext>> ResolveClinicContextAsync(
        Guid userId, List<string> roles, CancellationToken ct)
    {
        if (roles.Contains(UserRoles.ClinicOwner))
            return await ResolveOwnerContextAsync(userId, ct);

        if (roles.Contains(UserRoles.SuperAdmin))
            return Result.Success(LoginContext.Empty);

        return await ResolveStaffContextAsync(userId, ct);
    }

    private async Task<Result<LoginContext>> ResolveOwnerContextAsync(Guid userId, CancellationToken ct)
    {
        var clinic = await _uow.Clinics.GetByOwnerIdAsync(userId, ct);
        if (clinic is null)
            return Result.Success(LoginContext.Empty);

        // Owner may also be registered as a doctor — look up their member record
        var member = await _uow.Members.GetByUserIdIgnoreFiltersAsync(userId, ct);
        return Result.Success(new LoginContext(clinic.Id, member?.Id, clinic.CountryCode));
    }

    private async Task<Result<LoginContext>> ResolveStaffContextAsync(Guid userId, CancellationToken ct)
    {
        var staff = await _uow.Members.GetByUserIdIgnoreFiltersAsync(userId, ct);
        if (staff is null)
            return Result.Success(LoginContext.Empty);

        if (!staff.IsActive)
            return Result.Failure<LoginContext>(ErrorCodes.STAFF_INACTIVE,
                "Your account has been deactivated. Please contact your clinic owner.");

        var countryCode = await _uow.Clinics.GetCountryCodeAsync(staff.ClinicId, ct);
        return Result.Success(new LoginContext(staff.ClinicId, staff.Id, countryCode));
    }

    // ── Audit helper ──────────────────────────────────────────────────────────

    private Task AuditAsync(
        string eventName, string? detail,
        User user, Guid? clinicId, CancellationToken ct,
        List<string>? roles = null)
        => _audit.WriteEventAsync(eventName, detail,
            overrideUserId:   user.Id,
            overrideFullName: user.Person.FullName,
            overrideEmail:    user.Email,
            overrideRole:     roles is not null ? string.Join(",", roles) : null,
            overrideClinicId: clinicId,
            ct: ct);

    // ── Value objects ─────────────────────────────────────────────────────────

    private record LoginContext(Guid? ClinicId, Guid? MemberId, string? CountryCode)
    {
        public static readonly LoginContext Empty = new(null, null, null);
    }
}
