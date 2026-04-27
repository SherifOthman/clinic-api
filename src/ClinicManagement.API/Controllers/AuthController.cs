using ClinicManagement.API.Contracts.Auth;
using ClinicManagement.API.Models;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
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
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers;

[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly CookieService _cookieService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AuthController> _logger;

    public AuthController(CookieService cookieService, ICurrentUserService currentUser, ILogger<AuthController> logger)
    {
        _cookieService = cookieService;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Login with email/username and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.AuthLogin)]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
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

        _logger.LogInformation("Login successful for {Email}, isMobile={IsMobile}, RefreshToken={HasToken}",
            request.EmailOrUsername, isMobile, result.Value!.RefreshToken != null);

        if (!isMobile && result.Value!.RefreshToken != null)
        {
            _logger.LogInformation("Setting refresh token cookie for web client");
            _cookieService.SetRefreshTokenCookie(result.Value.RefreshToken);
        }

        return Ok(new TokenResponseDto(
            result.Value!.AccessToken,
            isMobile ? result.Value.RefreshToken : null));
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.AuthRegister)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand request, CancellationToken ct)
    {

        var result = await Sender.Send(request, ct);
        if (result.IsFailure)
            return HandleResult(result, "Registration Failed");

        return CreatedAtAction(nameof(GetMe), null);
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
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
    [EnableRateLimiting(RateLimitPolicies.AuthRefresh)]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest? request, CancellationToken ct)
    {
        var clientType = HttpContext.Request.Headers["X-Client-Type"].ToString();
        var isMobile = clientType.Equals("mobile", StringComparison.OrdinalIgnoreCase);

        var refreshToken = isMobile
            ? request?.RefreshToken
            : _cookieService.GetRefreshTokenFromCookie();

        _logger.LogInformation("RefreshToken called: isMobile={IsMobile}, HasRefreshToken={HasToken}",
            isMobile, !string.IsNullOrEmpty(refreshToken));

        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("RefreshToken failed: No refresh token provided");
            return Unauthorized();
        }

        var command = new RefreshTokenCommand(refreshToken, isMobile);
        var result = await Sender.Send(command, ct);

        if (result.IsFailure)
        {
            _logger.LogWarning("RefreshToken failed: {Error}", result.ErrorMessage);
            if (!isMobile) _cookieService.ClearRefreshTokenCookie();
            return Unauthorized();
        }

        if (!isMobile && result.Value!.RefreshToken != null)
        {
            _logger.LogInformation("Setting new refresh token cookie");
            _cookieService.SetRefreshTokenCookie(result.Value.RefreshToken);
        }

        return Ok(new TokenResponseDto(
            result.Value!.AccessToken,
            isMobile ? result.Value.RefreshToken : null
        ));
    }

    /// <summary>
    /// Confirm email address
    /// </summary>
    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.AuthConfirmEmail)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailCommand request, CancellationToken ct)
    {
        var result = await Sender.Send(request, ct);
        return HandleNoContent(result, "Email Confirmation Failed");
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.AuthForgotPassword)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand request, CancellationToken ct)
    {
        await Sender.Send(request, ct);

        return NoContent();
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.AuthResetPassword)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand request, CancellationToken ct)
    {
        var result = await Sender.Send(request, ct);
        return HandleNoContent(result, "Password Reset Failed");
    }

    /// <summary>
    /// Change current user password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand request, CancellationToken ct)
    {
        var result = await Sender.Send(request, ct);
        return HandleNoContent(result, "Password Change Failed");
    }

    /// <summary>
    /// Check if email is available for registration
    /// </summary>
    [HttpGet("check-email")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.AuthEnumeration)]
    [ProducesResponseType(typeof(AvailabilityDto), StatusCodes.Status200OK)]
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
    [EnableRateLimiting(RateLimitPolicies.AuthEnumeration)]
    [ProducesResponseType(typeof(AvailabilityDto), StatusCodes.Status200OK)]
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
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
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
    [EnableRateLimiting(RateLimitPolicies.AuthResendVerification)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendEmailVerification([FromBody] ResendEmailVerificationCommand request, CancellationToken ct)
    {
        var result = await Sender.Send(request, ct);
        return HandleNoContent(result, "Resend Failed");
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var command = new UpdateProfileCommand(request.FullName, request.UserName, request.PhoneNumber, request.Gender);
        var result = await Sender.Send(command, ct);
        return HandleNoContent(result, "Update Failed");
    }

    /// <summary>
    /// Upload profile image
    /// </summary>
    [HttpPut("profile/image")]
    [Authorize]
    [EnableRateLimiting(RateLimitPolicies.UserUpload)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadProfileImage(IFormFile file, CancellationToken ct)
    {
        var command = new UploadProfileImageCommand(file);
        var result = await Sender.Send(command, ct);
        return HandleNoContent(result, "Upload Failed");
    }

    /// <summary>
    /// Delete profile image
    /// </summary>
    [HttpDelete("profile/image")]
    [Authorize]
    [EnableRateLimiting(RateLimitPolicies.UserDeletes)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteProfileImage(CancellationToken ct)
    {
        var command = new DeleteProfileImageCommand();
        var result = await Sender.Send(command, ct);
        return HandleNoContent(result, "Delete Failed");
    }
}
