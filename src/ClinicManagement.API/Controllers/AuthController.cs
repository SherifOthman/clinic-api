using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Auth.Commands.Login;
using ClinicManagement.Application.Features.Auth.Commands.RefreshToken;
using ClinicManagement.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        SetRefreshTokenCookie(result.Value!.RefreshToken);
        return CreateTokenResponse(result.Value);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized("Refresh token not found");

        var result = await _mediator.Send(new RefreshTokenCommand { RefreshToken = refreshToken }, cancellationToken);

        if (!result.IsSuccess)
            return Unauthorized(result.Error);

        SetRefreshTokenCookie(result.Value!.RefreshToken);
        return CreateTokenResponse(result.Value);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("refreshToken");
        return Ok(new { message = "Logged out successfully" });
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("refreshToken", refreshToken, options);
    }

    private IActionResult CreateTokenResponse(AuthResponseDto auth)
    {
        return Ok(new
        {
            AccessToken = auth.AccessToken,
            User = auth.User
        });
    }
}
