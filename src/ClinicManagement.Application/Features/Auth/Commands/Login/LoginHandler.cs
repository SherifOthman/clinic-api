using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Auth.QueryModels;
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

        var (user, roles) = userResult.Value!;

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
    /// Validates the credentials and returns the authenticated user + roles.
    /// Roles are loaded in the same query as the user — no separate GetRolesAsync call.
    /// Stamps LastLoginAt before ResetAccessFailedCountAsync so both are saved
    /// in a single UPDATE — no second SaveChanges needed.
    /// </summary>
    private async Task<Result<UserWithRoles>> AuthenticateAsync(LoginCommand request, CancellationToken ct)
    {
        var result = await _uow.Users.GetByEmailOrUsernameWithRolesAsync(request.EmailOrUsername, ct);
        if (result is null)
        {
            _logger.LogWarning("Login failed — user not found: {Input}", request.EmailOrUsername);
            await _audit.WriteEventAsync("LoginFailed", $"User not found: {request.EmailOrUsername}", ct: ct);
            return Result.Failure<UserWithRoles>(ErrorCodes.INVALID_CREDENTIALS, "Invalid email/username or password");
        }

        var user = result.User;

        if (await _userManager.IsLockedOutAsync(user))
            return await FailLockedOutAsync(user, ct);

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            return await FailWrongPasswordAsync(user, ct);

        // Stamp LastLoginAt first — UserManager.ResetAccessFailedCountAsync calls
        // UpdateAsync internally, which saves all dirty properties in one UPDATE.
        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _userManager.ResetAccessFailedCountAsync(user);

        return Result.Success(result);
    }

    private async Task<Result<UserWithRoles>> FailLockedOutAsync(User user, CancellationToken ct)
    {
        var lockoutEnd       = await _userManager.GetLockoutEndDateAsync(user);
        var remainingMinutes = lockoutEnd.HasValue
            ? (int)(lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes + 1
            : 30;

        _logger.LogWarning("Login blocked — account locked: {UserId}", user.Id);
        await AuditAsync("LoginBlocked", $"Account locked, {remainingMinutes} min remaining", user, clinicId: null, ct);

        return Result.Failure<UserWithRoles>(ErrorCodes.ACCOUNT_LOCKED,
            $"Account is locked. Please try again in {remainingMinutes} minute(s).");
    }

    private async Task<Result<UserWithRoles>> FailWrongPasswordAsync(User user, CancellationToken ct)
    {
        _logger.LogWarning("Login failed — wrong password: {UserId}", user.Id);
        await _userManager.AccessFailedAsync(user);

        if (await _userManager.IsLockedOutAsync(user))
        {
            _logger.LogWarning("Account locked after failed attempts: {UserId}", user.Id);
            await AuditAsync("AccountLocked", "Locked after too many failed attempts", user, clinicId: null, ct);
            return Result.Failure<UserWithRoles>(ErrorCodes.ACCOUNT_LOCKED,
                "Account is locked due to multiple failed login attempts. Please try again in 30 minutes.");
        }

        await AuditAsync("LoginFailed", "Invalid password", user, clinicId: null, ct);
        return Result.Failure<UserWithRoles>(ErrorCodes.INVALID_CREDENTIALS, "Invalid email/username or password");
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

        // Owner may also be registered as a doctor — member record carries the MemberId for the JWT.
        // Use WithClinic variant so we get CountryCode from the already-loaded clinic nav property
        // instead of a second query. For owners the clinic is the same one we just loaded.
        var member = await _uow.Members.GetByUserIdWithClinicAsync(userId, ct);
        return Result.Success(new LoginContext(clinic.Id, member?.Id, clinic.CountryCode));
    }

    private async Task<Result<LoginContext>> ResolveStaffContextAsync(Guid userId, CancellationToken ct)
    {
        // Single query: member + clinic JOIN — gives us ClinicId, MemberId, and CountryCode
        var staff = await _uow.Members.GetByUserIdWithClinicAsync(userId, ct);
        if (staff is null)
            return Result.Success(LoginContext.Empty);

        if (!staff.IsActive)
            return Result.Failure<LoginContext>(ErrorCodes.STAFF_INACTIVE,
                "Your account has been deactivated. Please contact your clinic owner.");

        return Result.Success(new LoginContext(staff.ClinicId, staff.Id, staff.Clinic.CountryCode));
    }

    // ── Audit helper ──────────────────────────────────────────────────────────

    private Task AuditAsync(
        string eventName, string? detail,
        User user, Guid? clinicId, CancellationToken ct,
        List<string>? roles = null)
        => _audit.WriteEventAsync(eventName, detail,
            overrideUserId:   user.Id,
            overrideFullName: user.FullName,
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
