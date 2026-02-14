using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.API.Infrastructure.Services;

public class CookieService
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
        _cookieSettings = cookieSettings.Value;
        _logger = logger;
    }

    public void SetAccessTokenCookie(string accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            return;
        }

        var cookieOptions = CreateSecureCookieOptions(TimeSpan.FromMinutes(_cookieSettings.ExpiryInMinutes));
        _httpContextAccessor.HttpContext?.Response.Cookies.Append(CookieConstants.AccessToken, accessToken, cookieOptions);
        
        _logger.LogInformation("Access token cookie set: Secure={Secure}, SameSite={SameSite}, HttpOnly={HttpOnly}, Expiry={Expiry}", 
            cookieOptions.Secure, cookieOptions.SameSite, cookieOptions.HttpOnly, cookieOptions.Expires);
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
        var context = _httpContextAccessor.HttpContext;
        var token = context?.Request.Cookies[CookieConstants.AccessToken];
        
        _logger.LogInformation("Access token cookie retrieval: Found={Found}, Origin={Origin}", 
            !string.IsNullOrEmpty(token), context?.Request.Headers.Origin.FirstOrDefault() ?? "none");
            
        return token;
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
        
        _logger.LogInformation("Authentication cookies cleared");
    }

    private CookieOptions CreateSecureCookieOptions(TimeSpan expiry)
    {
        // Development: Use relaxed settings for HTTP
        // Production: Use strict settings for HTTPS
        var isProduction = _cookieSettings.IsProduction;
        
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction, // Only require HTTPS in production
            SameSite = isProduction ? SameSiteMode.None : SameSiteMode.Lax, // Lax for local dev
            Expires = DateTimeOffset.UtcNow.Add(expiry),
            Path = "/",
            IsEssential = true,
            Domain = null
        };
    }
}
