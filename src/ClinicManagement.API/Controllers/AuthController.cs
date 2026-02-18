using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Auth.Commands.ChangePassword;
using ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;
using ClinicManagement.Application.Features.Auth.Commands.DeleteProfileImage;
using ClinicManagement.Application.Features.Auth.Commands.ForgotPassword;
using ClinicManagement.Application.Features.Auth.Commands.Login;
using ClinicManagement.Application.Features.Auth.Commands.Logout;
using ClinicManagement.Application.Features.Auth.Commands.RefreshToken;
using ClinicManagement.Application.Features.Auth.Commands.Register;
using ClinicManagement.Application.Features.Auth.Commands.ResendEmailVerification;
using ClinicManagement.Application.Features.Auth.Commands.ResetPassword;
using ClinicManagement.Application.Features.Auth.Commands.UpdateProfile;
using ClinicManagement.Application.Features.Auth.Commands.UploadProfileImage;
using ClinicManagement.Application.Features.Auth.Queries.CheckEmailAvailability;
using ClinicManagement.Application.Features.Auth.Queries.CheckUsernameAvailability;
using ClinicManagement.Application.Features.Auth.Queries.GetMe;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiProblemDetails = ClinicManagement.Application.Common.Models.ApiProblemDetails;
using MessageResponse = ClinicManagement.Application.Common.Models.MessageResponse;
using CookieService = ClinicManagement.Infrastructure.Services.CookieService;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : BaseApiController
{
    private readonly CookieService _cookieService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(
        ISender sender,
        CookieService cookieService,
        ICurrentUserService currentUser,
        ILogger<AuthController> logger)
        : base(sender, logger)
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
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken ct)
    {
        var clientType = HttpContext.Request.Headers["X-Client-Type"].ToString();
        var isMobile = clientType.Equals("mobile", StringComparison.OrdinalIgnoreCase);

        var command = new LoginCommand(
            request.EmailOrUsername,
            request.Password,
            isMobile
        );

        var result = await Sender.Send(command, ct);

        if (!result.Success)
            return Error(result.ErrorCode!, result.ErrorMessage!, "Login Failed");

        // For web clients, set refresh token as HTTP-only cookie
        if (!isMobile && result.RefreshToken != null)
        {
            _cookieService.SetRefreshTokenCookie(result.RefreshToken);
        }

        return Ok(new LoginResponseDto(
            result.AccessToken!,
            isMobile ? result.RefreshToken : null,
            result.EmailNotConfirmed
        ));
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken ct)
    {
        var command = new RegisterCommand(
            request.Email,
            request.Username,
            request.Password,
            request.FirstName,
            request.LastName,
            request.PhoneNumber
        );

        var result = await Sender.Send(command, ct);

        if (!result.Success)
            return Error(result.ErrorCode!, result.ErrorMessage!, "Registration Failed");

        return CreatedAtAction(
            nameof(GetMe),
            new MessageResponse("Registration successful. Please check your email to verify your account.")
        );
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
        var query = new GetMeQuery(_currentUser.UserId!.Value);
        var result = await Sender.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RefreshTokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto? request, CancellationToken ct)
    {
        var clientType = HttpContext.Request.Headers["X-Client-Type"].ToString();
        var isMobile = clientType.Equals("mobile", StringComparison.OrdinalIgnoreCase);

        var refreshToken = isMobile
            ? request?.RefreshToken
            : _cookieService.GetRefreshTokenFromCookie();

        if (string.IsNullOrEmpty(refreshToken))
        {
            Logger.LogWarning("{ClientType} client refresh request with missing token", isMobile ? "Mobile" : "Web");
            return Unauthorized();
        }

        var command = new RefreshTokenCommand(refreshToken, isMobile);
        var result = await Sender.Send(command, ct);

        if (!result.Success)
        {
            Logger.LogWarning("Token refresh failed");
            if (!isMobile) _cookieService.ClearRefreshTokenCookie();
            return Unauthorized();
        }

        // For web clients, set new refresh token as HTTP-only cookie
        if (!isMobile && result.RefreshToken != null)
        {
            _cookieService.SetRefreshTokenCookie(result.RefreshToken);
        }

        return Ok(new RefreshTokenResponseDto(
            result.AccessToken!,
            isMobile ? result.RefreshToken : null
        ));
    }

    /// <summary>
    /// Confirm email address
    /// </summary>
    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequestDto request, CancellationToken ct)
    {
        var command = new ConfirmEmailCommand(request.Email, request.Token);
        var result = await Sender.Send(command, ct);

        if (!result.Success)
            return Error(result.ErrorCode!, result.ErrorMessage!, "Email Confirmation Failed");

        var message = result.AlreadyConfirmed
            ? "Email already confirmed"
            : "Email confirmed successfully";

        return Ok(new MessageResponse(message));
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request, CancellationToken ct)
    {
        var command = new ForgotPasswordCommand(request.Email);
        await Sender.Send(command, ct);

        // Always return success to prevent email enumeration
        return Ok(new MessageResponse("Password reset email sent"));
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request, CancellationToken ct)
    {
        var command = new ResetPasswordCommand(request.Email, request.Token, request.NewPassword);
        var result = await Sender.Send(command, ct);

        if (!result.Success)
            return Error(result.ErrorCode!, result.ErrorMessage!, "Password Reset Failed");

        return Ok(new MessageResponse("Password reset successfully"));
    }

    /// <summary>
    /// Change current user password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request, CancellationToken ct)
    {
        var command = new ChangePasswordCommand(request.CurrentPassword, request.NewPassword);
        var result = await Sender.Send(command, ct);

        if (!result.Success)
            return Error(result.ErrorCode!, result.ErrorMessage!, "Password Change Failed");

        return Ok(new MessageResponse("Password changed successfully"));
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
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto? request, CancellationToken ct)
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

        Logger.LogInformation("{ClientType} client logged out", isMobile ? "Mobile" : "Web");
        return Ok(new MessageResponse("Logged out successfully"));
    }

    /// <summary>
    /// Resend email verification
    /// </summary>
    [HttpPost("resend-email-verification")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendEmailVerification([FromBody] ResendEmailVerificationRequestDto request, CancellationToken ct)
    {
        var command = new ResendEmailVerificationCommand(request.Email);
        var result = await Sender.Send(command, ct);

        if (!result.Success)
            return Error(result.ErrorCode!, result.ErrorMessage!, "Resend Failed");

        return Ok(new MessageResponse("Verification email sent"));
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request, CancellationToken ct)
    {
        var command = new UpdateProfileCommand(request.FirstName, request.LastName, request.PhoneNumber);
        var result = await Sender.Send(command, ct);

        if (!result.Success)
            return Error(result.ErrorCode!, result.ErrorMessage!, "Update Failed");

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

        if (!result.Success)
            return Error(result.ErrorCode!, result.ErrorMessage!, "Upload Failed");

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

        if (!result.Success)
            return Error(result.ErrorCode!, result.ErrorMessage!, "Delete Failed");

        return NoContent();
    }
}

// DTOs for request/response
public record LoginRequestDto(string EmailOrUsername, string Password);
public record LoginResponseDto(string AccessToken, string? RefreshToken, bool? EmailNotConfirmed);
public record RegisterRequestDto(string Email, string Username, string Password, string FirstName, string LastName, string? PhoneNumber);
public record RefreshTokenRequestDto(string? RefreshToken);
public record RefreshTokenResponseDto(string AccessToken, string? RefreshToken);
public record ConfirmEmailRequestDto(string Email, string Token);
public record ForgotPasswordRequestDto(string Email);
public record ResetPasswordRequestDto(string Email, string Token, string NewPassword);
public record ChangePasswordRequestDto(string CurrentPassword, string NewPassword);
public record LogoutRequestDto(string? RefreshToken);
public record ResendEmailVerificationRequestDto(string Email);
public record UpdateProfileRequestDto(string FirstName, string LastName, string? PhoneNumber);
