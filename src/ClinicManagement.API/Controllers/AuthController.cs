using ClinicManagement.API.Extensions;
using ClinicManagement.API.Models;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Auth.Commands.ChangePassword;
using ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;
using ClinicManagement.Application.Features.Auth.Commands.ForgotPassword;
using ClinicManagement.Application.Features.Auth.Commands.Login;
using ClinicManagement.Application.Features.Auth.Commands.Logout;
using ClinicManagement.Application.Features.Auth.Commands.Register;
using ClinicManagement.Application.Features.Auth.Commands.ResendEmailVerification;
using ClinicManagement.Application.Features.Auth.Commands.ResetPassword;
using ClinicManagement.Application.Features.Auth.Queries.GetMe;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
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

    /// <summary>
    /// Authenticate user and set tokens as httpOnly cookies.
    /// Tokens are NEVER returned in the response body - only success message is returned.
    /// </summary>
    /// <param name="command">Login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message (tokens are set as httpOnly cookies)</returns>
    [HttpPost("login")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Success)
            return BadRequest(result.ToApiError());

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Logout user and clear refresh token cookie
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var command = new LogoutCommand();
        var result = await _mediator.Send(command, cancellationToken);
        
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Get current authenticated user.
    /// Token refresh is handled automatically by JwtCookieMiddleware.
    /// This endpoint simply returns the current user data.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current user data</returns>
    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize] // Require authentication
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var query = new GetMeQuery();
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.Success)
            return Unauthorized(new ApiError(result.Message));

        return Ok(result.Value);
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

    [HttpPost("resend-email-verification")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendEmailVerification(ResendEmailVerificationCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.Success)
            return Ok(new { message = result.Message });
        
        return BadRequest(result.ToApiError());
    }

    /// <summary>
    /// Change user password (requires authentication)
    /// </summary>
    /// <param name="command">Password change request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
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

}
