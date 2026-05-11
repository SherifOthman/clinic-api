using ClinicManagement.API.Contracts.Auth;
using ClinicManagement.API.Models;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Options;
using ClinicManagement.Application.Features.Auth.Commands.GoogleLogin;
using ClinicManagement.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace ClinicManagement.API.Controllers;

/// <summary>
/// Handles OAuth flows for both web and mobile clients.
///
/// Web flow (browser redirect + cookies):
///   1. GET  /api/auth/oauth/google          → Challenge Google
///   2. GET  /api/auth/oauth/google/complete → reads Cookie principal → sets JWT cookies → redirect
///
/// Mobile flow (native SDK + id_token):
///   1. Mobile app uses Google Sign-In SDK → gets id_token
///   2. POST /api/auth/oauth/google/mobile  → verifies id_token → returns JWT tokens in body
/// </summary>
[Route("api/auth/oauth")]
public class OAuthController : BaseApiController
{
    private readonly ICookieService _cookieService;
    private readonly AppOptions _appOptions;
    private readonly ILogger<OAuthController> _logger;
    private readonly int _accessTokenExpiryMinutes;

    public OAuthController(
        ICookieService cookieService,
        IOptions<AppOptions> appOptions,
        IOptions<JwtOptions> jwtOptions,
        ILogger<OAuthController> logger)
    {
        _cookieService            = cookieService;
        _appOptions               = appOptions.Value;
        _logger                   = logger;
        _accessTokenExpiryMinutes = jwtOptions.Value.AccessTokenExpirationMinutes;
    }

    // ── Web: Step 1 — Redirect to Google ─────────────────────────────────────

    [HttpGet("google")]
    public IActionResult GoogleLogin([FromQuery] string? returnUrl = null)
    {
        var dashboardUrl = _appOptions.DashboardUrl ?? "http://localhost:3000";
        var completeUrl  = Url.Action(nameof(GoogleComplete), "OAuth",
            new { returnUrl = returnUrl ?? dashboardUrl }, Request.Scheme)!;

        return Challenge(
            new AuthenticationProperties { RedirectUri = completeUrl },
            GoogleDefaults.AuthenticationScheme);
    }

    // ── Web: Step 3 — After Google callback, issue JWT cookies ───────────────

    [HttpGet("google/complete")]
    public async Task<IActionResult> GoogleComplete(
        [FromQuery] string? returnUrl, CancellationToken ct)
    {
        var dashboardUrl = returnUrl ?? _appOptions.DashboardUrl ?? "http://localhost:3000";
        var loginUrl     = GetLoginUrl();

        var auth = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!auth.Succeeded || auth.Principal is null)
        {
            _logger.LogWarning("Google OAuth complete: no cookie principal. {Error}", auth.Failure?.Message);
            return Redirect($"{loginUrl}?error=oauth_failed");
        }

        var email      = auth.Principal.FindFirstValue(ClaimTypes.Email);
        var fullName   = auth.Principal.FindFirstValue(ClaimTypes.Name) ?? "Google User";
        var googleId   = auth.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var pictureUrl = auth.Principal.FindFirstValue("urn:google:picture");

        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Google OAuth: no email in claims");
            return Redirect($"{loginUrl}?error=no_email");
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var result = await Sender.Send(new GoogleLoginCommand(email, fullName, googleId, pictureUrl), ct);

        if (result.IsFailure)
        {
            _logger.LogWarning("Google OAuth handler failed: {Code}", result.ErrorCode);
            return Redirect($"{loginUrl}?error={result.ErrorCode?.ToLower() ?? "unknown"}");
        }

        _cookieService.SetAccessTokenCookie(result.Value!.AccessToken!, _accessTokenExpiryMinutes);
        _cookieService.SetRefreshTokenCookie(result.Value.RefreshToken!);

        _logger.LogInformation("Google OAuth complete — redirecting to {Url}", dashboardUrl);
        return Redirect(dashboardUrl);
    }

    // ── Mobile: Verify id_token, return JWT tokens in body ───────────────────

    /// <summary>
    /// Google Sign-In for mobile apps.
    ///
    /// The mobile app uses the Google Sign-In SDK to obtain an id_token,
    /// then sends it here. We verify it with Google's tokeninfo endpoint,
    /// extract the user profile, and return JWT tokens in the response body.
    ///
    /// Requires X-Client-Type: mobile header.
    /// </summary>
    [HttpPost("google/mobile")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.AuthLogin)]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleMobileLogin(
        [FromBody] GoogleMobileLoginRequest request, CancellationToken ct)
    {
        var clientType = HttpContext.Request.Headers["X-Client-Type"].ToString();
        if (!clientType.Equals("mobile", StringComparison.OrdinalIgnoreCase))
            return BadRequest("This endpoint is for mobile clients only.");

        if (string.IsNullOrWhiteSpace(request.IdToken))
            return BadRequest("id_token is required.");

        var result = await Sender.Send(new GoogleMobileLoginCommand(request.IdToken), ct);

        if (result.IsFailure)
        {
            _logger.LogWarning("Google mobile login failed: {Code} — {Message}",
                result.ErrorCode, result.ErrorMessage);
            return HandleResult(result, "Google Sign-In failed");
        }

        _logger.LogInformation("Google mobile login successful");
        return Ok(new TokenResponseDto(result.Value!.AccessToken, result.Value.RefreshToken));
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private string GetLoginUrl()
        => $"{_appOptions.WebsiteUrl.TrimEnd('/')}/en/login";
}
