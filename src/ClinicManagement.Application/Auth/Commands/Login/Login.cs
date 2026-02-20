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
    string? RefreshToken,
    bool EmailNotConfirmed = false
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

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for {EmailOrUsername}", request.EmailOrUsername);
            return Result.Failure<LoginResponseDto>(ErrorCodes.INVALID_CREDENTIALS, "Invalid email/username or password");
        }

        var emailNotConfirmed = !user.IsEmailConfirmed;
        if (emailNotConfirmed)
        {
            _logger.LogInformation("User {UserId} logged in with unconfirmed email", user.Id);
        }

        var roles = await _unitOfWork.Users.GetUserRolesAsync(user.Id, cancellationToken);
        var staff = await _unitOfWork.Users.GetStaffByUserIdAsync(user.Id, cancellationToken);
        var clinicId = staff?.ClinicId;

        var accessToken = _tokenService.GenerateAccessToken(user, roles, clinicId);
        var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, null, cancellationToken);

        _logger.LogInformation("User {UserId} logged in ({ClientType}) with roles [{Roles}]",
            user.Id, request.IsMobile ? "mobile" : "web", string.Join(", ", roles));

        var response = new LoginResponseDto(
            AccessToken: accessToken,
            RefreshToken: refreshToken.Token,
            EmailNotConfirmed: emailNotConfirmed
        );

        return Result.Success(response);
    }
}

