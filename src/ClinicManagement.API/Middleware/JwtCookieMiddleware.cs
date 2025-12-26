using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ClinicManagement.API.Middleware;

/// <summary>
/// JWT Cookie Middleware - Handles token refresh for cookie-based authentication.
/// Works in conjunction with JWT Bearer authentication.
/// </summary>
public class JwtCookieMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtCookieMiddleware> _logger;
    private readonly JwtOptions _jwtOptions;

    public JwtCookieMiddleware(RequestDelegate next, ILogger<JwtCookieMiddleware> logger, IOptions<JwtOptions> jwtOptions)
    {
        _next = next;
        _logger = logger;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task InvokeAsync(HttpContext context, IAuthenticationService authService, ICookieService cookieService)
    {
        // Skip middleware for public endpoints
        if (IsPublicEndpoint(context))
        {
            await _next(context);
            return;
        }

        try
        {
            await HandleTokenRefreshAsync(context, authService, cookieService);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in JWT Cookie Middleware");
        }

        await _next(context);
    }

    private async Task HandleTokenRefreshAsync(HttpContext context, IAuthenticationService authService, ICookieService cookieService)
    {
        // Only handle token refresh if we have cookies but no valid access token
        var accessToken = cookieService.GetAccessTokenFromCookie();
        var refreshToken = cookieService.GetRefreshTokenFromCookie();

        // If no cookies, let JWT Bearer handle Authorization header
        if (string.IsNullOrEmpty(accessToken) && string.IsNullOrEmpty(refreshToken))
        {
            return;
        }

        // Try to validate access token
        var principal = authService.ValidateAccessToken(accessToken);
        if (principal != null)
        {
            // Access token is valid, no need to refresh
            return;
        }

        // Access token is invalid/expired, try refresh token
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var refreshResult = await authService.RefreshTokenAsync(refreshToken);
            if (refreshResult.Success && refreshResult.Value != null)
            {
                // Refresh successful - set new cookies
                var result = refreshResult.Value;
                cookieService.SetAccessTokenCookie(result.AccessToken);
                cookieService.SetRefreshTokenCookie(result.RefreshToken);
                
                _logger.LogInformation("Token refreshed successfully for user {UserId}", 
                    result.UserPrincipal.FindFirst("sub")?.Value);
            }
            else
            {
                // Refresh failed - clear cookies
                cookieService.ClearAuthCookies();
                _logger.LogDebug("Token refresh failed - clearing cookies");
            }
        }
        else
        {
            // No refresh token - clear cookies
            cookieService.ClearAuthCookies();
            _logger.LogDebug("No refresh token available - clearing cookies");
        }
    }

    private bool IsPublicEndpoint(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null) return true;

        // Check if endpoint allows anonymous access
        var allowAnonymous = endpoint.Metadata.GetMetadata<AllowAnonymousAttribute>();
        if (allowAnonymous != null) return true;

        // Check if endpoint requires authorization
        var authorizeData = endpoint.Metadata.GetMetadata<AuthorizeAttribute>();
        if (authorizeData == null) return true; // No authorization required

        // Use public paths from JWT options (Application layer)
        var path = context.Request.Path.Value?.ToLowerInvariant();
        return _jwtOptions.PublicPaths.Any(publicPath => 
            path?.StartsWith(publicPath.ToLowerInvariant()) == true);
    }
}