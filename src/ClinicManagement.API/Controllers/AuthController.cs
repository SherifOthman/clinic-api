using ClinicManagement.API.Contracts.Auth;
using ClinicManagement.API.Models;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Auth.Commands;
using ClinicManagement.Application.Features.Auth.Commands.ChangePassword;
using ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;
using ClinicManagement.Application.Features.Auth.Commands.ForgotPassword;
using ClinicManagement.Application.Features.Auth.Commands.Login;
using ClinicManagement.Application.Features.Auth.Commands.Register;
using ClinicManagement.Application.Features.Auth.Commands.ResendEmailVerification;
using ClinicManagement.Application.Features.Auth.Commands.ResetPassword;
using ClinicManagement.Application.Features.Auth.Commands.UpdateProfile;
using ClinicManagement.Application.Features.Auth.Queries;
using ClinicManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly CookieService _cookieService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(CookieService cookieService, ICurrentUserService currentUser)
    {
        _cookieService = cookieService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Login with email/username and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var clientType = HttpContext.Request.Headers["X-Client-Type"].ToString();
        var isMobile = clientType.Equals("mobile", StringComparison.OrdinalIgnoreCase);

        var command = new LoginCommand(
            request.EmailOrUsername,
            request.Password,
            isMobile
        );

        var result = await Sender.Send(command, ct);

        if (result.IsFailure)
            return HandleResult(result, "Login Failed");

        var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AuthController>>();
        logger.LogInformation("Login successful for {Email}, isMobile={IsMobile}, RefreshToken={HasToken}", 
            request.EmailOrUsername, isMobile, result.Value!.RefreshToken != null);

        if (!isMobile && result.Value!.RefreshToken != null)
        {
            logger.LogInformation("Setting refresh token cookie for web client");
            _cookieService.SetRefreshTokenCookie(result.Value.RefreshToken);
        }

        return Ok(new LoginResponseDto(
            result.Value!.AccessToken,
            isMobile ? result.Value.RefreshToken : null));
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var command = new RegisterCommand(
            request.FirstName,
            request.LastName,
            request.UserName,
            request.Email,
            request.Password,
            request.Gender,
            request.PhoneNumber
        );
        
        var result = await Sender.Send(command, ct);
        if (result.IsFailure)
            return HandleResult(result, "Registration Failed");

        return CreatedAtAction(nameof(GetMe), null);
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(GetMeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var userId = _currentUser.GetRequiredUserId();
        var query = new GetMeQuery(userId);
        var result = await Sender.Send(query, ct);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RefreshTokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest? request, CancellationToken ct)
    {
        var clientType = HttpContext.Request.Headers["X-Client-Type"].ToString();
        var isMobile = clientType.Equals("mobile", StringComparison.OrdinalIgnoreCase);

        var refreshToken = isMobile
            ? request?.RefreshToken
            : _cookieService.GetRefreshTokenFromCookie();

        var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AuthController>>();
        logger.LogInformation("RefreshToken called: isMobile={IsMobile}, HasRefreshToken={HasToken}", 
            isMobile, !string.IsNullOrEmpty(refreshToken));

        if (string.IsNullOrEmpty(refreshToken))
        {
            logger.LogWarning("RefreshToken failed: No refresh token provided");
            return Unauthorized();
        }

        var command = new RefreshTokenCommand(refreshToken, isMobile);
        var result = await Sender.Send(command, ct);

        if (result.IsFailure)
        {
            logger.LogWarning("RefreshToken failed: {Error}", result.ErrorMessage);
            if (!isMobile) _cookieService.ClearRefreshTokenCookie();
            return Unauthorized();
        }

        if (!isMobile && result.Value!.RefreshToken != null)
        {
            logger.LogInformation("Setting new refresh token cookie");
            _cookieService.SetRefreshTokenCookie(result.Value.RefreshToken);
        }

        return Ok(new RefreshTokenResponseDto(
            result.Value!.AccessToken,
            isMobile ? result.Value.RefreshToken : null
        ));
    }

    /// <summary>
    /// Confirm email address
    /// </summary>
    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request, CancellationToken ct)
    {
        var command = new ConfirmEmailCommand(request.Email, request.Token);
        var result = await Sender.Send(command, ct);

        if (result.IsFailure)
            return HandleResult(result, "Email Confirmation Failed");

        return NoContent();
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        var command = new ForgotPasswordCommand(request.Email);
        await Sender.Send(command, ct);

        return NoContent();
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var command = new ResetPasswordCommand(request.Email, request.Token, request.NewPassword);
        var result = await Sender.Send(command, ct);

        if (result.IsFailure)
            return HandleResult(result, "Password Reset Failed");

        return NoContent();
    }

    /// <summary>
    /// Change current user password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var command = new ChangePasswordCommand(request.CurrentPassword, request.NewPassword);
        var result = await Sender.Send(command, ct);

        if (result.IsFailure)
            return HandleResult(result, "Password Change Failed");

        return NoContent();
    }

    /// <summary>
    /// Check if email is available for registration
    /// </summary>
    [HttpGet("check-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CheckEmailAvailabilityDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckEmailAvailability([FromQuery] string email, CancellationToken ct)
    {
        var query = new CheckEmailAvailabilityQuery(email);
        var result = await Sender.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Check if username is available for registration
    /// </summary>
    [HttpGet("check-username")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CheckUsernameAvailabilityDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckUsernameAvailability([FromQuery] string username, CancellationToken ct)
    {
        var query = new CheckUsernameAvailabilityQuery(username);
        var result = await Sender.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Logout current user
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest? request, CancellationToken ct)
    {
        var clientType = HttpContext.Request.Headers["X-Client-Type"].ToString();
        var isMobile = clientType.Equals("mobile", StringComparison.OrdinalIgnoreCase);

        string? refreshToken = isMobile
            ? request?.RefreshToken
            : _cookieService.GetRefreshTokenFromCookie();

        var command = new LogoutCommand(refreshToken);
        await Sender.Send(command, ct);

        if (!isMobile)
        {
            _cookieService.ClearRefreshTokenCookie();
        }

        return NoContent();
    }

    /// <summary>
    /// Resend email verification
    /// </summary>
    [HttpPost("resend-email-verification")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendEmailVerification([FromBody] ResendEmailVerificationRequest request, CancellationToken ct)
    {
        var command = new ResendEmailVerificationCommand(request.Email);
        var result = await Sender.Send(command, ct);

        if (result.IsFailure)
            return HandleResult(result, "Resend Failed");

        return NoContent();
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var command = new UpdateProfileCommand(request.FirstName, request.LastName,request.UserName, request.PhoneNumber);
        var result = await Sender.Send(command, ct);

        if (result.IsFailure)
            return HandleResult(result, "Update Failed");

        return NoContent();
    }

    /// <summary>
    /// Upload profile image
    /// </summary>
    [HttpPost("profile/image/upload")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadProfileImage(IFormFile file, CancellationToken ct)
    {
        var command = new UploadProfileImageCommand(file);
        var result = await Sender.Send(command, ct);

        if (result.IsFailure)
            return HandleResult(result, "Upload Failed");

        return NoContent();
    }

    /// <summary>
    /// Delete profile image
    /// </summary>
    [HttpDelete("profile/image")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteProfileImage(CancellationToken ct)
    {
        var command = new DeleteProfileImageCommand();
        var result = await Sender.Send(command, ct);

        if (result.IsFailure)
            return HandleResult(result, "Delete Failed");

        return NoContent();
    }
}
