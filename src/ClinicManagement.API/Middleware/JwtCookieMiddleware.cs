using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ClinicManagement.API.Middleware;

/// <summary>
/// JWT Cookie Middleware - Handles automatic token refresh for cookie-based authentication.
/// 
/// Purpose: Refresh expired (but valid) access tokens BEFORE JWT Bearer authentication runs.
/// This ensures users with expired tokens get seamless re-authentication.
/// 
/// Security: Only refreshes tokens that are expired, NOT invalid/malformed tokens.
/// </summary>
public class JwtCookieMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtCookieMiddleware> _logger;

    public JwtCookieMiddleware(RequestDelegate next, ILogger<JwtCookieMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITokenService tokenService, IAuthenticationService authService, ICookieService cookieService)
    {
        // Skip token validation and refresh for endpoints marked with [AllowAnonymous]
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
        {
            await _next(context);
            return;
        }

        var accessToken = cookieService.GetAccessTokenFromCookie();
        var refreshToken = cookieService.GetRefreshTokenFromCookie();

        // Only attempt refresh if we have both tokens and access token is expired
        if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
        {
            var (principal, isExpired) = tokenService.ValidateAccessTokenWithExpiry(accessToken);
            
            // Only refresh if token is EXPIRED (not invalid)
            // This is a security check - we don't refresh malformed/tampered tokens
            if (principal == null && isExpired)
            {
                await TryRefreshTokenAsync(authService, cookieService, refreshToken);
            }
        }

        await _next(context);
    }

    private async Task TryRefreshTokenAsync(IAuthenticationService authService, ICookieService cookieService, string refreshToken)
    {
        try
        {
            var refreshResult = await authService.RefreshTokenAsync(refreshToken);
            
            if (refreshResult.Success && refreshResult.Value != null)
            {
                var result = refreshResult.Value;
                cookieService.SetAccessTokenCookie(result.AccessToken);
                cookieService.SetRefreshTokenCookie(result.RefreshToken);
                
                _logger.LogDebug("Access token refreshed successfully");
            }
            // If refresh fails, don't clear cookies - let JWT Bearer return 401
            // The frontend will handle the 401 and redirect to login
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            // Don't clear cookies on error - let the request continue
        }
    }
}