using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

public class CookieService : ICookieService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CookieSettings _cookieSettings;

    public CookieService(
        IHttpContextAccessor httpContextAccessor,
        IOptions<CookieSettings> cookieSettings)
    {
        _httpContextAccessor = httpContextAccessor;
        _cookieSettings      = cookieSettings.Value;
    }

    // ── Access token ──────────────────────────────────────────────────────────

    public void SetAccessTokenCookie(string accessToken, int expiryMinutes)
    {
        if (string.IsNullOrEmpty(accessToken)) return;
        _httpContextAccessor.HttpContext?.Response.Cookies.Append(
            CookieConstants.AccessToken,
            accessToken,
            CreateSecureCookieOptions(TimeSpan.FromMinutes(expiryMinutes)));
    }

    public string? GetAccessTokenFromCookie()
        => _httpContextAccessor.HttpContext?.Request.Cookies[CookieConstants.AccessToken];

    // ── Refresh token ─────────────────────────────────────────────────────────

    public void SetRefreshTokenCookie(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken)) return;
        _httpContextAccessor.HttpContext?.Response.Cookies.Append(
            CookieConstants.RefreshToken,
            refreshToken,
            CreateSecureCookieOptions(TimeSpan.FromDays(_cookieSettings.RefreshTokenExpiryInDays)));
    }

    public string? GetRefreshTokenFromCookie()
        => _httpContextAccessor.HttpContext?.Request.Cookies[CookieConstants.RefreshToken];

    // ── Clear both ────────────────────────────────────────────────────────────

    public void ClearAllAuthCookies()
    {
        var expired = CreateSecureCookieOptions(TimeSpan.FromDays(-1));
        _httpContextAccessor.HttpContext?.Response.Cookies.Append(CookieConstants.AccessToken,  "", expired);
        _httpContextAccessor.HttpContext?.Response.Cookies.Append(CookieConstants.RefreshToken, "", expired);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private CookieOptions CreateSecureCookieOptions(TimeSpan expiry)
    {
        var isProduction = _cookieSettings.IsProduction;
        return new CookieOptions
        {
            HttpOnly    = true,
            Secure      = isProduction,
            SameSite    = isProduction ? SameSiteMode.None : SameSiteMode.Lax,
            Expires     = DateTimeOffset.UtcNow.Add(expiry),
            Domain      = _cookieSettings.CookieDomain,  // e.g. ".yourapp.com" for cross-subdomain
            Path        = "/",
            IsEssential = true,
        };
    }
}
