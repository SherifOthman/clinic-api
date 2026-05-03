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
    private readonly ITokenIssuer _tokenIssuer;
    private readonly IAuditWriter _audit;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IUnitOfWork uow,
        UserManager<User> userManager,
        ITokenIssuer tokenIssuer,
        IAuditWriter audit,
        ILogger<LoginHandler> logger)
    {
        _uow         = uow;
        _userManager = userManager;
        _tokenIssuer = tokenIssuer;
        _audit       = audit;
        _logger      = logger;
    }

    public async Task<Result<TokenResponseDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var userResult = await AuthenticateAsync(request, ct);
        if (userResult.IsFailure)
            return Result.Failure<TokenResponseDto>(userResult.ErrorCode!, userResult.ErrorMessage!);

        var (user, roles) = userResult.Value!;

        var contextResult = await _tokenIssuer.ResolveContextAsync(user.Id, roles, ct);
        if (contextResult.IsFailure)
        {
            await AuditAsync("LoginFailed", contextResult.ErrorMessage, user, clinicId: null, ct);
            return Result.Failure<TokenResponseDto>(contextResult.ErrorCode!, contextResult.ErrorMessage!);
        }

        var tokens = await _tokenIssuer.IssueTokenPairAsync(user, roles, contextResult.Value!, ct);

        await AuditAsync("LoginSuccess", null, user, contextResult.Value!.ClinicId, ct, roles);
        _logger.LogInformation("User {UserId} logged in ({Mode}) [{Roles}]",
            user.Id, request.IsMobile ? "mobile" : "web", string.Join(", ", roles));

        return Result.Success(tokens);
    }

    // ── Authentication ────────────────────────────────────────────────────────

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
}
