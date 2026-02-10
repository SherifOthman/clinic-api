using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.Login;

public record LoginCommand : IRequest<Result>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode(MessageCodes.Fields.EMAIL_REQUIRED);

        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode(MessageCodes.Fields.PASSWORD_REQUIRED);
    }
}

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
            return Result.Fail(loginResult.Code!);

        var result = loginResult.Value!;

        // Set tokens as httpOnly cookies - NEVER return in response body
        _cookieService.SetAccessTokenCookie(result.AccessToken);
        _cookieService.SetRefreshTokenCookie(result.RefreshToken);

        // Return success only - no message needed
        return Result.Ok();
    }
}