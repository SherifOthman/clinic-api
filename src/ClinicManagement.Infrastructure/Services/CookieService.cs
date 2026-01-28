using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Infrastructure.Common.Constants;
using ClinicManagement.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

public class CookieService : ICookieService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CookieSettings _cookieSettings;

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
        
        _httpContextAccessor.HttpContext?.Response.Cookies.Append(CookieConstants.AccessToken, accessToken, cookieOptions);
        
        // Log cookie setting for debugging
        var context = _httpContextAccessor.HttpContext;
        var logger = context?.RequestServices.GetService<ILogger<CookieService>>();
        logger?.LogDebug("Access token cookie set with expiry: {Expiry} minutes", _cookieSettings.ExpiryInMinutes);
    }

    public void SetRefreshTokenCookie(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return;
        }

        var cookieOptions = CreateSecureCookieOptions(TimeSpan.FromDays(_cookieSettings.RefreshTokenExpiryInDays));
        
        _httpContextAccessor.HttpContext?.Response.Cookies.Append(CookieConstants.RefreshToken, refreshToken, cookieOptions);
    }

    public string? GetAccessTokenFromCookie()
    {
        return _httpContextAccessor.HttpContext?.Request.Cookies[CookieConstants.AccessToken];
    }

    public string? GetRefreshTokenFromCookie()
    {
        return _httpContextAccessor.HttpContext?.Request.Cookies[CookieConstants.RefreshToken];
    }

    public void ClearAuthCookies()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        var expiredOptions = CreateSecureCookieOptions(TimeSpan.FromDays(-1));
        
        context.Response.Cookies.Append(CookieConstants.AccessToken, "", expiredOptions);
        context.Response.Cookies.Append(CookieConstants.RefreshToken, "", expiredOptions);
        
        // Log cookie clearing for debugging
        var logger = context.RequestServices.GetService<ILogger<CookieService>>();
        logger?.LogInformation("Authentication cookies cleared");
    }

    private CookieOptions CreateSecureCookieOptions(TimeSpan expiry)
    {
        var context = _httpContextAccessor.HttpContext;
        var isHttps = context?.Request.IsHttps ?? false;
        
        return new CookieOptions
        {
            HttpOnly = true,                    // Prevents XSS attacks
            Secure = _cookieSettings.IsProduction && isHttps, // Only require HTTPS in production
            SameSite = _cookieSettings.IsProduction ? SameSiteMode.None : SameSiteMode.Lax, // Cross-origin support
            Expires = DateTimeOffset.UtcNow.Add(expiry),
            Path = "/",                         // Available to entire application
            IsEssential = true,                 // GDPR compliance - essential for authentication
            Domain = _cookieSettings.IsProduction ? _cookieSettings.CookieDomain : null // Set domain in production
        };
    }
}
