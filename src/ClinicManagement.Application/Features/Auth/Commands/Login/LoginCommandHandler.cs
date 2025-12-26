using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.Login;

/// <summary>
/// Handler for LoginCommand - SIMPLIFIED for Clean Architecture.
/// Delegates authentication logic to IAuthenticationService.
/// Only handles command validation and cookie setting.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result>
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

    public async Task<Result> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Delegate authentication logic to Application layer service
        var loginResult = await _authenticationService.LoginAsync(request.Email, request.Password, cancellationToken);

        if (!loginResult.Success)
            return Result.Fail(loginResult.Message);

        var result = loginResult.Value!;

        // Set tokens as httpOnly cookies - NEVER return in response body
        _cookieService.SetAccessTokenCookie(result.AccessToken);
        _cookieService.SetRefreshTokenCookie(result.RefreshToken);

        // Return success only - frontend calls /me to get user data
        return Result.Ok("Login successful");
    }
}