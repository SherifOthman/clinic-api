using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Abstractions.Storage;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Auth.Commands.Login;

public record LoginCommand(
    string EmailOrUsername,
    string Password,
    bool IsMobile
) : IRequest<Result<LoginResponseDto>>;

public record LoginResponseDto(
    string AccessToken,
    string? RefreshToken
);

public class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService,
        ILogger<LoginHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailOrUsernameAsync(request.EmailOrUsername, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Failed login attempt for {EmailOrUsername} - user not found", request.EmailOrUsername);
            return Result.Failure<LoginResponseDto>(ErrorCodes.INVALID_CREDENTIALS, "Invalid email/username or password");
        }

        // Check if account is locked out
        if (user.LockoutEndDate.HasValue && user.LockoutEndDate.Value > DateTime.UtcNow)
        {
            var remainingMinutes = (int)(user.LockoutEndDate.Value - DateTime.UtcNow).TotalMinutes + 1;
            _logger.LogWarning("Login attempt for locked account {UserId}, lockout ends in {Minutes} minutes", 
                user.Id, remainingMinutes);
            return Result.Failure<LoginResponseDto>(
                ErrorCodes.ACCOUNT_LOCKED, 
                $"Account is locked due to multiple failed login attempts. Please try again in {remainingMinutes} minute(s).");
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for {EmailOrUsername} - invalid password", request.EmailOrUsername);
            
            // Increment failed login attempts
            await _unitOfWork.Users.IncrementFailedLoginAttemptsAsync(user.Id, cancellationToken);
            
            // Check if this was the 5th failed attempt
            if (user.FailedLoginAttempts + 1 >= 5)
            {
                _logger.LogWarning("Account {UserId} locked after 5 failed login attempts", user.Id);
                return Result.Failure<LoginResponseDto>(
                    ErrorCodes.ACCOUNT_LOCKED, 
                    "Account is locked due to multiple failed login attempts. Please try again in 30 minutes.");
            }
            
            return Result.Failure<LoginResponseDto>(ErrorCodes.INVALID_CREDENTIALS, "Invalid email/username or password");
        }

        // Reset failed login attempts on successful login
        if (user.FailedLoginAttempts > 0 || user.LockoutEndDate.HasValue)
        {
            await _unitOfWork.Users.ResetFailedLoginAttemptsAsync(user.Id, cancellationToken);
        }

        // Update last login timestamp
        await _unitOfWork.Users.UpdateLastLoginAsync(user.Id, cancellationToken);

        var emailNotConfirmed = !user.IsEmailConfirmed;
        if (emailNotConfirmed)
        {
            _logger.LogInformation("User {UserId} logged in with unconfirmed email", user.Id);
        }

        var roles = await _unitOfWork.Users.GetUserRolesAsync(user.Id, cancellationToken);
        
        // Get ClinicId - check if user is a clinic owner first, then check if they're staff
        Guid? clinicId = null;
        if (roles.Contains(Roles.ClinicOwner))
        {
            var clinic = await _unitOfWork.Clinics.GetByOwnerUserIdAsync(user.Id, cancellationToken);
            clinicId = clinic?.Id;
        }
        else
        {
            var staff = await _unitOfWork.Users.GetStaffByUserIdAsync(user.Id, cancellationToken);
            clinicId = staff?.ClinicId;
        }

        var accessToken = _tokenService.GenerateAccessToken(user, roles, clinicId);
        var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, null, cancellationToken);

        _logger.LogInformation("User {UserId} logged in ({ClientType}) with roles [{Roles}]",
            user.Id, request.IsMobile ? "mobile" : "web", string.Join(", ", roles));

        var response = new LoginResponseDto(
            AccessToken: accessToken,
            RefreshToken: refreshToken.Token
        );

        return Result.Success(response);
    }
}

