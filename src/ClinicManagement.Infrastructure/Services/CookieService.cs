using ClinicManagement.Application.Common.Options;
using ClinicManagement.Domain.Common.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Service for managing HTTP-only refresh token cookies
/// Access tokens are now sent via Authorization header
/// </summary>
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

    /// <summary>
    /// Sets refresh token in HTTP-only cookie (web clients only)
    /// </summary>
    public void SetRefreshTokenCookie(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("Attempted to set empty refresh token cookie");
            return;
        }

        var cookieOptions = CreateSecureCookieOptions(TimeSpan.FromDays(_cookieSettings.RefreshTokenExpiryInDays));
        _httpContextAccessor.HttpContext?.Response.Cookies.Append(
            CookieConstants.RefreshToken, 
            refreshToken, 
            cookieOptions);
        
        _logger.LogInformation("Refresh token cookie set");
    }

    /// <summary>
    /// Gets refresh token from HTTP-only cookie (web clients only)
    /// </summary>
    public string? GetRefreshTokenFromCookie()
    {
        return _httpContextAccessor.HttpContext?.Request.Cookies[CookieConstants.RefreshToken];
    }

    /// <summary>
    /// Clears refresh token cookie (web clients only)
    /// </summary>
    public void ClearRefreshTokenCookie()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            _logger.LogWarning("Cannot clear cookie: HttpContext is null");
            return;
        }

        var expiredOptions = CreateSecureCookieOptions(TimeSpan.FromDays(-1));
        context.Response.Cookies.Append(CookieConstants.RefreshToken, "", expiredOptions);
        
        _logger.LogInformation("Refresh token cookie cleared");
    }

    private CookieOptions CreateSecureCookieOptions(TimeSpan expiry)
    {
        var isProduction = _cookieSettings.IsProduction;
        
        return new CookieOptions
        {
            HttpOnly = true, // Prevents JavaScript access (XSS protection)
            Secure = isProduction, // HTTPS only in production
            SameSite = isProduction ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.Add(expiry),
            Path = "/",
            IsEssential = true,
            Domain = null
        };
    }
}
