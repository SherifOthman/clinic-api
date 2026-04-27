using ClinicManagement.Application.Common.Options;
using ClinicManagement.Application.Features.Auth.Commands.GoogleLogin;
using ClinicManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace ClinicManagement.API.Controllers;

/// <summary>
/// Thin controller — only handles the OAuth redirect/callback.
/// All business logic (find/create user, generate tokens) lives in GoogleLoginHandler.
/// </summary>
[Route("api/auth/oauth")]
public class OAuthController : BaseApiController
{
    private readonly CookieService _cookieService;
    private readonly AppOptions _appOptions;
    private readonly ILogger<OAuthController> _logger;

    public OAuthController(
        CookieService cookieService,
        IOptions<AppOptions> appOptions,
        ILogger<OAuthController> logger)
    {
        _cookieService = cookieService;
        _appOptions    = appOptions.Value;
        _logger        = logger;
    }

    // ── Step 1: Redirect to Google ────────────────────────────────────────────

    [HttpGet("google")]
    public IActionResult GoogleLogin([FromQuery] string? returnUrl = null)
    {
        var dashboardUrl = _appOptions.FrontendUrl ?? "http://localhost:3000";
        var callbackUrl  = Url.Action(nameof(GoogleCallback), "OAuth",
            new { returnUrl = returnUrl ?? dashboardUrl }, Request.Scheme)!;

        return Challenge(
            new AuthenticationProperties { RedirectUri = callbackUrl },
            GoogleDefaults.AuthenticationScheme);
    }

    // ── Step 2: Google redirects back here ────────────────────────────────────

    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback(
        [FromQuery] string? returnUrl, CancellationToken ct)
    {
        var dashboardUrl = returnUrl ?? _appOptions.FrontendUrl ?? "http://localhost:3000";
        var loginUrl     = GetLoginUrl();

        // Authenticate with Google scheme to extract the user's claims
        var auth = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (!auth.Succeeded || auth.Principal is null)
        {
            _logger.LogWarning("Google OAuth failed: {Error}", auth.Failure?.Message);
            return Redirect($"{loginUrl}?error=oauth_failed");
        }

        var email    = auth.Principal.FindFirstValue(ClaimTypes.Email);
        var fullName = auth.Principal.FindFirstValue(ClaimTypes.Name) ?? "Google User";
        var googleId = auth.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Google OAuth: no email in claims");
            return Redirect($"{loginUrl}?error=no_email");
        }

        // Delegate all business logic to the handler
        var result = await Sender.Send(new GoogleLoginCommand(email, fullName, googleId), ct);

        if (result.IsFailure)
        {
            _logger.LogWarning("Google OAuth handler failed: {Code}", result.ErrorCode);
            return Redirect($"{loginUrl}?error={result.ErrorCode?.ToLower() ?? "unknown"}");
        }

        // Set both tokens as HttpOnly cookies — same as credentials login
        _cookieService.SetAccessTokenCookie(result.Value!.AccessToken!, 60);
        _cookieService.SetRefreshTokenCookie(result.Value.RefreshToken!);

        return Redirect(dashboardUrl);
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private string GetLoginUrl()
        => $"{_appOptions.AuthUrl.TrimEnd('/')}/en/login";
}
