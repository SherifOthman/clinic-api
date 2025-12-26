using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Enterprise-grade cookie service with security hardening
/// Handles all authentication cookie operations with proper security controls
/// </summary>
public class CookieService : ICookieService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CookieSettings _cookieSettings;

    // Cookie names as constants for consistency
    private const string AccessTokenCookieName = "accessToken";
    private const string RefreshTokenCookieName = "refreshToken";

    public CookieService(IHttpContextAccessor httpContextAccessor, IOptions<CookieSettings> cookieSettings)
    {
        _httpContextAccessor = httpContextAccessor;
        _cookieSettings = cookieSettings.Value;
    }

    public void SetAccessTokenCookie(string accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            return;
        }

        var cookieOptions = CreateSecureCookieOptions(TimeSpan.FromMinutes(_cookieSettings.ExpiryInMinutes));
        
        _httpContextAccessor.HttpContext?.Response.Cookies.Append(AccessTokenCookieName, accessToken, cookieOptions);
    }

    public void SetRefreshTokenCookie(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return;
        }

        var cookieOptions = CreateSecureCookieOptions(TimeSpan.FromDays(_cookieSettings.RefreshTokenExpiryInDays));
        
        _httpContextAccessor.HttpContext?.Response.Cookies.Append(RefreshTokenCookieName, refreshToken, cookieOptions);
    }

    public string? GetAccessTokenFromCookie()
    {
        return _httpContextAccessor.HttpContext?.Request.Cookies[AccessTokenCookieName];
    }

    public string? GetRefreshTokenFromCookie()
    {
        return _httpContextAccessor.HttpContext?.Request.Cookies[RefreshTokenCookieName];
    }

    public void ClearAuthCookies()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        // Create expired cookie options to clear cookies
        var expiredOptions = CreateSecureCookieOptions(TimeSpan.FromDays(-1));
        
        // Clear both cookies
        context.Response.Cookies.Append(AccessTokenCookieName, "", expiredOptions);
        context.Response.Cookies.Append(RefreshTokenCookieName, "", expiredOptions);
    }

    private CookieOptions CreateSecureCookieOptions(TimeSpan expiry)
    {
        var context = _httpContextAccessor.HttpContext;
        var isHttps = context?.Request.IsHttps ?? false;
        
        return new CookieOptions
        {
            HttpOnly = true,                    // Prevents XSS attacks
            Secure = isHttps,                   // Use HTTPS detection instead of production flag
            SameSite = _cookieSettings.IsProduction ? SameSiteMode.None : SameSiteMode.Lax, // Cross-origin support
            Expires = DateTimeOffset.UtcNow.Add(expiry),
            Path = "/",                         // Available to entire application
            IsEssential = true,                 // GDPR compliance - essential for authentication
            Domain = _cookieSettings.IsProduction ? _cookieSettings.CookieDomain : null // Set domain in production
        };
    }
}
