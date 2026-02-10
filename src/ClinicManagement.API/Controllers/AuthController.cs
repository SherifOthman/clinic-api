using ClinicManagement.API.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Auth.Commands.ChangePassword;
using ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;
using ClinicManagement.Application.Features.Auth.Commands.DeleteProfileImage;
using ClinicManagement.Application.Features.Auth.Commands.ForgotPassword;
using ClinicManagement.Application.Features.Auth.Commands.Login;
using ClinicManagement.Application.Features.Auth.Commands.Logout;
using ClinicManagement.Application.Features.Auth.Commands.Register;
using ClinicManagement.Application.Features.Auth.Commands.ResendEmailVerification;
using ClinicManagement.Application.Features.Auth.Commands.ResetPassword;
using ClinicManagement.Application.Features.Auth.Commands.UpdateProfile;
using ClinicManagement.Application.Features.Auth.Commands.UpdateProfileImage;
using ClinicManagement.Application.Features.Auth.Commands.UploadProfileImage;
using ClinicManagement.Application.Features.Auth.Queries.GetMe;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var command = new LogoutCommand();
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var query = new GetMeQuery();
        var result = await _mediator.Send(query, cancellationToken);
        
        if (!result.Success)
            return Unauthorized();
            
        return Ok(result.Value);
    }

    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("resend-email-verification")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendEmailVerification(ResendEmailVerificationCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile(UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("profile/image")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfileImage(UpdateProfileImageCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("profile/image/upload")]
    [Authorize]
    [DisableRequestSizeLimit]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadProfileImage(IFormFile file, CancellationToken cancellationToken)
    {
        var command = new UploadProfileImageCommand { File = file };
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("profile/image")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteProfileImage(CancellationToken cancellationToken)
    {
        var command = new DeleteProfileImageCommand();
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
