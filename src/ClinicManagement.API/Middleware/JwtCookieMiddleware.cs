using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ClinicManagement.API.Middleware;

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

        // Handle different token scenarios
        if (!string.IsNullOrEmpty(accessToken))
        {
            // We have an access token - check if it's valid or expired
            var (principal, isExpired) = tokenService.ValidateAccessTokenWithExpiry(accessToken);
            
            if (principal == null)
            {
                // Token is invalid or expired
                if (isExpired && !string.IsNullOrEmpty(refreshToken))
                {
                    // Token is expired but we have refresh token - try to refresh
                    var refreshSuccess = await TryRefreshTokenAsync(authService, cookieService, refreshToken);
                    if (!refreshSuccess)
                    {
                        // Refresh failed - clear cookies to force re-login
                        cookieService.ClearAuthCookies();
                        _logger.LogWarning("Token refresh failed, cookies cleared");
                    }
                }
                else
                {
                    // Token is invalid and no refresh token - clear cookies
                    cookieService.ClearAuthCookies();
                    _logger.LogWarning("Invalid access token without refresh token, cookies cleared");
                }
            }
            // If token is valid, continue with request
        }
        else if (!string.IsNullOrEmpty(refreshToken))
        {
            // We have only refresh token but no access token - try to refresh
            var refreshSuccess = await TryRefreshTokenAsync(authService, cookieService, refreshToken);
            if (!refreshSuccess)
            {
                // Refresh failed - clear cookies
                cookieService.ClearAuthCookies();
                _logger.LogWarning("Token refresh failed with refresh token only, cookies cleared");
            }
        }
        // If no tokens at all, let the request continue and JWT Bearer will return 401

        await _next(context);
    }

    private async Task<bool> TryRefreshTokenAsync(IAuthenticationService authService, ICookieService cookieService, string refreshToken)
    {
        try
        {
            var refreshResult = await authService.RefreshTokenAsync(refreshToken);
            
            if (refreshResult.Success && refreshResult.Value != null)
            {
                var result = refreshResult.Value;
                cookieService.SetAccessTokenCookie(result.AccessToken);
                cookieService.SetRefreshTokenCookie(result.RefreshToken);
                
                _logger.LogInformation("Access token refreshed successfully");
                return true;
            }
            else
            {
                _logger.LogWarning("Token refresh failed: {Message}", refreshResult.Message);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return false;
        }
    }
}