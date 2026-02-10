using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.Logout;

public class LogoutCommand : IRequest<Result>
{
    // No properties needed - uses CurrentUserService
}

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ICookieService _cookieService;

    public LogoutCommandHandler(IAuthenticationService authenticationService, ICookieService cookieService)
    {
        _authenticationService = authenticationService;
        _cookieService = cookieService;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // Get refresh token from cookie before clearing
        var refreshToken = _cookieService.GetRefreshTokenFromCookie();
        
        // Delegate logout logic to Application layer service
        await _authenticationService.LogoutAsync(refreshToken, cancellationToken);
        
        // Clear authentication cookies
        _cookieService.ClearAuthCookies();
        
        return Result.Ok();
    }
}