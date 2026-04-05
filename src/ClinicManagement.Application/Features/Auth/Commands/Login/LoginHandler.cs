using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IApplicationDbContext context,
        UserManager<Domain.Entities.User> userManager,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService,
        ICurrentUserService currentUserService,
        ILogger<LoginHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var ip = _currentUserService.IpAddress;

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.EmailOrUsername || u.UserName == request.EmailOrUsername, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Failed login attempt for {EmailOrUsername} - user not found", request.EmailOrUsername);
            await WriteSecurityAudit(null, null, null, "LoginFailed", $"User not found: {request.EmailOrUsername}", ip, cancellationToken);
            return Result.Failure<LoginResponseDto>(ErrorCodes.INVALID_CREDENTIALS, "Invalid email/username or password");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
            var remainingMinutes = lockoutEnd.HasValue
                ? (int)(lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes + 1
                : 30;

            _logger.LogWarning("Login attempt for locked account {UserId}", user.Id);
            await WriteSecurityAudit(user, null, null, "LoginBlocked", $"Account locked, {remainingMinutes} min remaining", ip, cancellationToken);
            return Result.Failure<LoginResponseDto>(ErrorCodes.ACCOUNT_LOCKED,
                $"Account is locked due to multiple failed login attempts. Please try again in {remainingMinutes} minute(s).");
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            _logger.LogWarning("Failed login attempt for {EmailOrUsername} - invalid password", request.EmailOrUsername);
            await _userManager.AccessFailedAsync(user);

            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Account {UserId} locked after multiple failed login attempts", user.Id);
                await WriteSecurityAudit(user, null, null, "AccountLocked", "Locked after too many failed attempts", ip, cancellationToken);
                return Result.Failure<LoginResponseDto>(ErrorCodes.ACCOUNT_LOCKED,
                    "Account is locked due to multiple failed login attempts. Please try again in 30 minutes.");
            }

            await WriteSecurityAudit(user, null, null, "LoginFailed", "Invalid password", ip, cancellationToken);
            return Result.Failure<LoginResponseDto>(ErrorCodes.INVALID_CREDENTIALS, "Invalid email/username or password");
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);

        Guid? clinicId = null;
        if (roles.Contains(Roles.ClinicOwner))
        {
            var clinic = await _context.Clinics
                .IgnoreQueryFilters([QueryFilterNames.Tenant])
                .FirstOrDefaultAsync(c => c.OwnerUserId == user.Id, cancellationToken);
            clinicId = clinic?.Id;
        }
        else
        {
            var staff = await _context.Staff
                .IgnoreQueryFilters([QueryFilterNames.Tenant])
                .FirstOrDefaultAsync(s => s.UserId == user.Id, cancellationToken);
            clinicId = staff?.ClinicId;
        }

        var accessToken = _tokenService.GenerateAccessToken(user, roles.ToList(), clinicId);
        var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, null, cancellationToken);

        await WriteSecurityAudit(user, clinicId, string.Join(",", roles), "LoginSuccess", null, ip, cancellationToken);

        _logger.LogInformation("User {UserId} logged in ({ClientType}) with roles [{Roles}]",
            user.Id, request.IsMobile ? "mobile" : "web", string.Join(", ", roles));

        return Result.Success(new LoginResponseDto(accessToken, refreshToken.Token));
    }

    private async Task WriteSecurityAudit(
        Domain.Entities.User? user,
        Guid? clinicId,
        string? role,
        string action,
        string? details,
        string? ip,
        CancellationToken ct)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            Timestamp  = DateTime.UtcNow,
            ClinicId   = clinicId,
            UserId     = user?.Id,
            FullName   = user != null ? $"{user.FirstName} {user.LastName}".Trim() : null,
            Username   = user?.UserName,            UserEmail  = user?.Email,
            UserRole   = role,
            UserAgent  = _currentUserService.UserAgent,
            EntityType = "Auth",
            EntityId   = user?.Id.ToString() ?? "unknown",
            Action     = AuditAction.Security,
            IpAddress  = ip,
            Changes    = details != null ? $"{{\"event\":\"{action}\",\"detail\":\"{details}\"}}" : $"{{\"event\":\"{action}\"}}",
        });
        await _context.SaveChangesAsync(ct);
    }
}
