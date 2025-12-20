using ClinicManagement.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Enterprise-grade cookie service with security hardening
/// Handles all authentication cookie operations with proper security controls
/// </summary>
public class CookieService : ICookieService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CookieService> _logger;

    // Cookie names as constants for consistency
    private const string AccessTokenCookieName = "accessToken";
    private const string RefreshTokenCookieName = "refreshToken";

    public CookieService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILogger<CookieService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _logger = logger;
    }

    public void SetAccessTokenCookie(string accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogWarning("Attempted to set empty access token cookie");
            return;
        }

        var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryInMinutes", 15);
        var cookieOptions = CreateSecureCookieOptions(TimeSpan.FromMinutes(expiryMinutes));
        
        _httpContextAccessor.HttpContext?.Response.Cookies.Append(AccessTokenCookieName, accessToken, cookieOptions);
        _logger.LogDebug("Access token cookie set with {ExpiryMinutes} minute expiry", expiryMinutes);
    }

    public void SetRefreshTokenCookie(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("Attempted to set empty refresh token cookie");
            return;
        }

        var expiryDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpiryInDays", 7);
        var cookieOptions = CreateSecureCookieOptions(TimeSpan.FromDays(expiryDays));
        
        _httpContextAccessor.HttpContext?.Response.Cookies.Append(RefreshTokenCookieName, refreshToken, cookieOptions);
        _logger.LogDebug("Refresh token cookie set with {ExpiryDays} day expiry", expiryDays);
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
        
        _logger.LogInformation("Authentication cookies cleared");
    }

    private CookieOptions CreateSecureCookieOptions(TimeSpan expiry)
    {
        var isProduction = _configuration.GetValue<bool>("IsProduction", false);
        
        return new CookieOptions
        {
            HttpOnly = true,                    // Prevents XSS attacks
            Secure = isProduction,              // HTTPS only in production
            SameSite = isProduction ? SameSiteMode.None : SameSiteMode.Lax, // Cross-origin support
            Expires = DateTimeOffset.UtcNow.Add(expiry),
            Path = "/",                         // Available to entire application
            IsEssential = true,                 // GDPR compliance - essential for authentication
            Domain = isProduction ? GetProductionDomain() : null // Set domain in production
        };
    }

    private string? GetProductionDomain()
    {
        // Configure your production domain here
        return _configuration.GetValue<string>("CookieDomain");
    }
}
