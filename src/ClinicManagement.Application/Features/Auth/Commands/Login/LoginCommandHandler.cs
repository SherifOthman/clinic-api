using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ICookieService _cookieService;

    public LoginCommandHandler(
        IAuthenticationService authenticationService,
        ICookieService cookieService)
    {
        _authenticationService = authenticationService;
        _cookieService = cookieService;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Delegate authentication logic to Application layer service
        var loginResult = await _authenticationService.LoginAsync(request.Email, request.Password, cancellationToken);

        if (!loginResult.Success)
            return Result<LoginResponseDto>.Fail(loginResult.Code!);

        var result = loginResult.Value!;

        // Set tokens as httpOnly cookies for backward compatibility
        _cookieService.SetAccessTokenCookie(result.AccessToken);
        _cookieService.SetRefreshTokenCookie(result.RefreshToken);

        // Also return tokens in response body for cross-origin support
        var response = new LoginResponseDto
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30) // Match token expiration
        };

        return Result<LoginResponseDto>.Ok(response);
    }
}
