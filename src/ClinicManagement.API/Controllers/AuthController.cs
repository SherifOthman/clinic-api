using ClinicManagement.API.Extensions;
using ClinicManagement.API.Models;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Auth.Commands.ChangePassword;
using ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;
using ClinicManagement.Application.Features.Auth.Commands.ForgotPassword;
using ClinicManagement.Application.Features.Auth.Commands.Login;
using ClinicManagement.Application.Features.Auth.Commands.RefreshToken;
using ClinicManagement.Application.Features.Auth.Commands.Register;
using ClinicManagement.Application.Features.Auth.Commands.ResetPassword;
using ClinicManagement.Application.Features.Auth.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICookieService _cookieService;

    public AuthController(IMediator mediator, ICookieService cookieService)
    {
        _mediator = mediator;
        _cookieService = cookieService;
    }

    [HttpPost("register")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterCommand command, CancellationToken cancellationToken)
    {
        // Register user without clinic - they'll complete onboarding after email verification
        command.Role = Domain.Common.Enums.UserRole.ClinicOwner;
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Success)
            return BadRequest(result.ToApiError());

        return Ok(new { 
            message = "Registration successful! Please check your email to verify your account before continuing with onboarding.",
            email = command.Email
        });
    }

    [HttpPost("login")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand 
        { 
            Email = request.Email, 
            Password = request.Password 
        };
        
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Success)
            return BadRequest(result.ToApiError());

        // Set both tokens as secure HttpOnly cookies
        _cookieService.SetAccessTokenCookie(result.Value!.AccessToken);
        _cookieService.SetRefreshTokenCookie(result.Value!.RefreshToken);

        return Ok(new { message = "Login successful" });
    }

    // Refresh token endpoint removed - refresh is now handled internally by middleware

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        _cookieService.ClearAuthCookies();
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized(Result.Fail("Invalid user token").ToApiError());

        var query = new GetCurrentUserQuery { UserId = userId };
        var result = await _mediator.Send(query, cancellationToken);

        return result.Success
            ? Ok(result.Value)
            : Unauthorized(result.ToApiError());
    }

    [HttpPost("confirm-email")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError),StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
       var result = await _mediator.Send(command, cancellationToken);

        if (result.Success)
            return Ok(new { message = result.Message });
        return BadRequest(result.ToApiError());
    }

    [HttpPost("forgot-password")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.Success)
            return Ok(new { message = result.Message });
        
        return BadRequest(result.ToApiError());
    }

    [HttpPost("reset-password")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.Success)
            return Ok(new { message = result.Message });
        
        return BadRequest(result.ToApiError());
    }

    [HttpPost("change-password")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.Success)
            return Ok(new { message = result.Message });
        
        return BadRequest(result.ToApiError());
    }


    [HttpPost("switch-clinic/{clinicId}")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SwitchClinic(int clinicId, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new ApiError("Invalid user token"));

        var command = new Application.Features.Auth.Commands.SwitchClinic.SwitchClinicCommand
        {
            UserId = userId,
            ClinicId = clinicId
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Success)
            return BadRequest(result.ToApiError());

        // Set new tokens as secure HttpOnly cookies
        _cookieService.SetAccessTokenCookie(result.Value!.AccessToken);
        _cookieService.SetRefreshTokenCookie(result.Value!.RefreshToken);

        return Ok(new { message = "Switched clinic successfully" });
    }
}

// Request models
public record LoginRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
