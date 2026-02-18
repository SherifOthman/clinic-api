using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Auth.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        UserManager<User> userManager,
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService,
        ILogger<LoginHandler> logger)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Try to find user by email first, then by username
        var user = await _userManager.FindByEmailAsync(request.EmailOrUsername);
        if (user == null)
        {
            user = await _userManager.FindByNameAsync(request.EmailOrUsername);
        }

        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            _logger.LogWarning("Failed login attempt for {EmailOrUsername}", request.EmailOrUsername);
            return new LoginResult(
                Success: false,
                AccessToken: null,
                RefreshToken: null,
                ErrorCode: ErrorCodes.INVALID_CREDENTIALS,
                ErrorMessage: "Invalid email/username or password"
            );
        }

        // Note: We allow login even if email is not confirmed
        // Frontend will handle redirecting to email verification page
        var emailNotConfirmed = !user.EmailConfirmed;
        if (emailNotConfirmed)
        {
            _logger.LogInformation("User {UserId} logged in with unconfirmed email", user.Id);
        }

        var roles = await _userManager.GetRolesAsync(user);

        // Get ClinicId from Staff table (if user is clinic staff)
        // SuperAdmin has no Staff record, so ClinicId will be null
        var staff = await _unitOfWork.Users.GetStaffByUserIdAsync(user.Id, cancellationToken);
        var clinicId = staff?.ClinicId;

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user, roles, clinicId);
        var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, null, cancellationToken);

        _logger.LogInformation("User {UserId} logged in ({ClientType}) with roles [{Roles}]",
            user.Id, request.IsMobile ? "mobile" : "web", string.Join(", ", roles));

        // For mobile clients, return refresh token in response body
        // For web clients, refresh token will be set as HTTP-only cookie by endpoint
        return new LoginResult(
            Success: true,
            AccessToken: accessToken,
            RefreshToken: request.IsMobile ? refreshToken.Token : null,
            ErrorCode: null,
            ErrorMessage: null,
            EmailNotConfirmed: emailNotConfirmed
        );
    }
}
