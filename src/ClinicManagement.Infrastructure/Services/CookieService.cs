using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

public class CookieService : ICookieService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CookieSettings _cookieSettings;
    private readonly ILogger<CookieService> _logger;

    public CookieService(
        IHttpContextAccessor httpContextAccessor,
        IOptions<CookieSettings> cookieSettings,
        ILogger<CookieService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _cookieSettings      = cookieSettings.Value;
        _logger              = logger;
    }

    // ── Access token ──────────────────────────────────────────────────────────

    public void SetAccessTokenCookie(string accessToken, int expiryMinutes)
    {
        if (string.IsNullOrEmpty(accessToken)) return;
        var options = CreateSecureCookieOptions(TimeSpan.FromMinutes(expiryMinutes));
        _httpContextAccessor.HttpContext?.Response.Cookies.Append(
            CookieConstants.AccessToken, accessToken, options);
    }

    public string? GetAccessTokenFromCookie()
        => _httpContextAccessor.HttpContext?.Request.Cookies[CookieConstants.AccessToken];

    public void ClearAccessTokenCookie()
        => _httpContextAccessor.HttpContext?.Response.Cookies.Append(
            CookieConstants.AccessToken, "", CreateSecureCookieOptions(TimeSpan.FromDays(-1)));

    // ── Refresh token ─────────────────────────────────────────────────────────

    public void SetRefreshTokenCookie(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken)) return;
        var options = CreateSecureCookieOptions(TimeSpan.FromDays(_cookieSettings.RefreshTokenExpiryInDays));
        _httpContextAccessor.HttpContext?.Response.Cookies.Append(
            CookieConstants.RefreshToken, refreshToken, options);
    }

    public string? GetRefreshTokenFromCookie()
        => _httpContextAccessor.HttpContext?.Request.Cookies[CookieConstants.RefreshToken];

    public void ClearRefreshTokenCookie()
        => _httpContextAccessor.HttpContext?.Response.Cookies.Append(
            CookieConstants.RefreshToken, "", CreateSecureCookieOptions(TimeSpan.FromDays(-1)));

    // ── Clear both ────────────────────────────────────────────────────────────

    public void ClearAllAuthCookies()
    {
        ClearAccessTokenCookie();
        ClearRefreshTokenCookie();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private CookieOptions CreateSecureCookieOptions(TimeSpan expiry)
    {
        var isProduction = _cookieSettings.IsProduction;
        return new CookieOptions
        {
            HttpOnly  = true,
            Secure    = isProduction,
            SameSite  = isProduction ? SameSiteMode.None : SameSiteMode.Lax,
            Expires   = DateTimeOffset.UtcNow.Add(expiry),
            Domain    = _cookieSettings.CookieDomain,   // e.g. ".yourapp.com" for cross-subdomain
            Path      = "/",
            IsEssential = true,
        };
    }
}
