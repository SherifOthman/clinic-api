using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Auth.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IApplicationDbContext context,
        UserManager<Domain.Entities.User> userManager,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService,
        ILogger<LoginHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.EmailOrUsername || u.UserName == request.EmailOrUsername, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Failed login attempt for {EmailOrUsername} - user not found", request.EmailOrUsername);
            return Result.Failure<LoginResponseDto>(ErrorCodes.INVALID_CREDENTIALS, "Invalid email/username or password");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
            var remainingMinutes = lockoutEnd.HasValue 
                ? (int)(lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes + 1 
                : 30;
                
            _logger.LogWarning("Login attempt for locked account {UserId}, lockout ends in {Minutes} minutes", 
                user.Id, remainingMinutes);
            return Result.Failure<LoginResponseDto>(
                ErrorCodes.ACCOUNT_LOCKED, 
                $"Account is locked due to multiple failed login attempts. Please try again in {remainingMinutes} minute(s).");
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            _logger.LogWarning("Failed login attempt for {EmailOrUsername} - invalid password", request.EmailOrUsername);
            
            await _userManager.AccessFailedAsync(user);
            
            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Account {UserId} locked after multiple failed login attempts", user.Id);
                return Result.Failure<LoginResponseDto>(
                    ErrorCodes.ACCOUNT_LOCKED, 
                    "Account is locked due to multiple failed login attempts. Please try again in 30 minutes.");
            }
            
            return Result.Failure<LoginResponseDto>(ErrorCodes.INVALID_CREDENTIALS, "Invalid email/username or password");
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        if (!user.EmailConfirmed)
        {
            _logger.LogInformation("User {UserId} logged in with unconfirmed email", user.Id);
        }

        var roles = await _userManager.GetRolesAsync(user);
        
        Guid? clinicId = null;
        if (roles.Contains(Roles.ClinicOwner))
        {
            var clinic = await _context.Clinics
                .FirstOrDefaultAsync(c => c.OwnerUserId == user.Id, cancellationToken);
            clinicId = clinic?.Id;
        }
        else
        {
            var staff = await _context.Staff
                .FirstOrDefaultAsync(s => s.UserId == user.Id, cancellationToken);
            clinicId = staff?.ClinicId;
        }

        var accessToken = _tokenService.GenerateAccessToken(user, roles.ToList(), clinicId);
        var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, null, cancellationToken);

        _logger.LogInformation("User {UserId} logged in ({ClientType}) with roles [{Roles}]",
            user.Id, request.IsMobile ? "mobile" : "web", string.Join(", ", roles));

        var response = new LoginResponseDto(accessToken, refreshToken.Token);

        return Result.Success(response);
    }
}
