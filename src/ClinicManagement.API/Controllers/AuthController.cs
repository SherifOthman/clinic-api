using ClinicManagement.API.Extensions;
using ClinicManagement.API.Models;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Auth.Commands.ConfrimEmail;
using ClinicManagement.Application.Features.Auth.Commands.Login;
using ClinicManagement.Application.Features.Auth.Commands.RefreshToken;
using ClinicManagement.Application.Features.Auth.Commands.Register;
using ClinicManagement.Application.Options;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly JwtOption _options;

    public AuthController(IMediator mediator, IOptions<JwtOption> options)
    {
        _mediator = mediator;
        _options = options.Value ;
    }

    [HttpPost("register-owner")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterOwner(RegisterCommand command, CancellationToken cancellationToken)
    {
        command.Role = Domain.Common.Enums.UserRole.ClinicOwner;
        var result = await _mediator.Send(command, cancellationToken);


        return result.Success
            ? NoContent()
            : BadRequest(result.ToApiError());
    }

    [HttpPost("login")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Success)
            return BadRequest(result.ToApiError());

        return CreateAuthResponse(result);
    }

    [HttpPost("refresh-token")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(Result.Fail("Refresh token not found")
                .ToApiError());

        var result = await _mediator.Send(new RefreshTokenCommand { RefreshToken = refreshToken }, cancellationToken);

        if (!result.Success)
            return Unauthorized(result.ToApiError());

        return CreateAuthResponse(result);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        var cookieOptions = new CookieOptions
        {
            Secure = true,
            SameSite = SameSiteMode.None
        };

        Response.Cookies.Delete("refreshToken", cookieOptions);
        Response.Cookies.Delete("accessToken", cookieOptions);

        return Ok(new { message = "Logged out successfully" });
    }

    [HttpGet("confrim-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError),StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail(ConfrimEmailCommand command, CancellationToken cancellationToken)
    {
       var result = await _mediator.Send(command, cancellationToken);

        if (result.Success)
            return Ok(result.Message);
        return BadRequest(result.ToApiError());
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(_options.RefreshTokenExpirationDays)
        };

        Response.Cookies.Append("refreshToken", refreshToken, options);
    }

    private void SetAccessTokenCookie(string accessToken)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes) 
        };

        Response.Cookies.Append("accessToken", accessToken, options);
    }
    
    private IActionResult CreateAuthResponse(Result<AuthResponseDto> result)
    {
        var clientType = Request.Headers["X-Client-Type"].FirstOrDefault() ?? "react";

        if (clientType.Equals("nextjs", StringComparison.OrdinalIgnoreCase))
        {
            SetRefreshTokenCookie(result.Value!.RefreshToken);
            SetAccessTokenCookie(result.Value!.AccessToken);

            return Ok(new { User = result.Value.User });
        }
        else
        {
            SetRefreshTokenCookie(result.Value!.RefreshToken);
            return Ok(new
            {
                AccessToken = result.Value.AccessToken,
                User = result.Value.User
            });
        }
    }
}
