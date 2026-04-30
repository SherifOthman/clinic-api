using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Options;
using ClinicManagement.Application.Features.Auth.Commands.GoogleLogin;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace ClinicManagement.API.Controllers;

/// <summary>
/// Thin controller — only handles the OAuth redirect/callback.
/// All business logic (find/create user, generate tokens) lives in GoogleLoginHandler.
///
/// Flow:
///   1. GET /api/auth/oauth/google          → Challenge Google → Google redirects to CallbackPath
///   2. ASP.NET Google handler handles CallbackPath internally, exchanges code for tokens,
///      signs in via the Cookie scheme, then redirects to RedirectUri
///   3. GET /api/auth/oauth/google/complete  → reads Cookie principal → issues JWT cookies → redirect to dashboard
/// </summary>
[Route("api/auth/oauth")]
public class OAuthController : BaseApiController
{
    private readonly ICookieService _cookieService;
    private readonly AppOptions _appOptions;
    private readonly ILogger<OAuthController> _logger;

    public OAuthController(
        ICookieService cookieService,
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

        // RedirectUri points to our /complete endpoint (not the CallbackPath)
        // returnUrl is passed as a query param to /complete
        var completeUrl = Url.Action(nameof(GoogleComplete), "OAuth",
            new { returnUrl = returnUrl ?? dashboardUrl }, Request.Scheme)!;

        return Challenge(
            new AuthenticationProperties { RedirectUri = completeUrl },
            GoogleDefaults.AuthenticationScheme);
    }

    // ── Step 3: After Google handler signs in via Cookie, we land here ─────────
    // The Google handler has already validated the code and signed the user into
    // the Cookie scheme. We read the principal from the cookie, issue JWT cookies,
    // then sign out of the temporary cookie.

    [HttpGet("google/complete")]
    public async Task<IActionResult> GoogleComplete(
        [FromQuery] string? returnUrl, CancellationToken ct)
    {
        var dashboardUrl = returnUrl ?? _appOptions.FrontendUrl ?? "http://localhost:3000";
        var loginUrl     = GetLoginUrl();

        // Read the principal that the Google handler stored in the Cookie scheme
        var auth = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!auth.Succeeded || auth.Principal is null)
        {
            _logger.LogWarning("Google OAuth complete: no cookie principal. {Error}", auth.Failure?.Message);
            return Redirect($"{loginUrl}?error=oauth_failed");
        }

        var email    = auth.Principal.FindFirstValue(ClaimTypes.Email);
        var fullName = auth.Principal.FindFirstValue(ClaimTypes.Name) ?? "Google User";
        var googleId = auth.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        // Google sends the profile picture as a custom claim
        var pictureUrl = auth.Principal.FindFirstValue("urn:google:picture");

        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Google OAuth: no email in claims");
            return Redirect($"{loginUrl}?error=no_email");
        }

        // Clean up the temporary OAuth cookie
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Delegate all business logic to the handler
        var result = await Sender.Send(new GoogleLoginCommand(email, fullName, googleId, pictureUrl), ct);

        if (result.IsFailure)
        {
            _logger.LogWarning("Google OAuth handler failed: {Code}", result.ErrorCode);
            return Redirect($"{loginUrl}?error={result.ErrorCode?.ToLower() ?? "unknown"}");
        }

        // Set both tokens as HttpOnly cookies — same as credentials login
        _cookieService.SetAccessTokenCookie(result.Value!.AccessToken!, 60);
        _cookieService.SetRefreshTokenCookie(result.Value.RefreshToken!);

        _logger.LogInformation("Google OAuth complete — redirecting to {Url}", dashboardUrl);
        return Redirect(dashboardUrl);
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private string GetLoginUrl()
        => $"{_appOptions.AuthUrl.TrimEnd('/')}/en/login";
}
