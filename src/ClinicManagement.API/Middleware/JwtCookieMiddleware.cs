using ClinicManagement.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;

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
        if (ShouldSkipTokenValidation(context))
        {
            await _next(context);
            return;
        }

        var accessToken = cookieService.GetAccessTokenFromCookie();
        var refreshToken = cookieService.GetRefreshTokenFromCookie();

        await HandleTokenValidationAsync(tokenService, authService, cookieService, accessToken, refreshToken);

        await _next(context);
    }

    private bool ShouldSkipTokenValidation(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        return endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null;
    }

    private async Task HandleTokenValidationAsync(
        ITokenService tokenService, 
        IAuthenticationService authService, 
        ICookieService cookieService, 
        string? accessToken, 
        string? refreshToken)
    {
        if (HasAccessToken(accessToken))
        {
            await HandleAccessTokenScenarioAsync(tokenService, authService, cookieService, accessToken!, refreshToken);
        }
        else if (HasRefreshToken(refreshToken))
        {
            await HandleRefreshTokenOnlyScenarioAsync(authService, cookieService, refreshToken!);
        }
        // If no tokens at all, let the request continue and JWT Bearer will return 401
    }

    private bool HasAccessToken(string? token) => !string.IsNullOrEmpty(token);
    
    private bool HasRefreshToken(string? token) => !string.IsNullOrEmpty(token);

    private async Task HandleAccessTokenScenarioAsync(
        ITokenService tokenService, 
        IAuthenticationService authService, 
        ICookieService cookieService, 
        string accessToken, 
        string? refreshToken)
    {
        var validationResult = tokenService.ValidateAccessTokenWithExpiry(accessToken);

        if (validationResult.IsValid)
        {
            // Token is valid, continue with request
            return;
        }

        // Token is invalid or expired
        if (HasRefreshToken(refreshToken))
        {
            // Try to refresh regardless of whether token is expired or invalid
            var logMessage = validationResult.IsExpired 
                ? "Access token expired, attempting refresh" 
                : "Access token invalid, attempting refresh";
            await TryRefreshOrClearCookiesAsync(authService, cookieService, refreshToken!, logMessage);
        }
        else
        {
            // No refresh token available, clear cookies
            var reason = validationResult.IsExpired 
                ? "Access token expired without refresh token" 
                : "Access token invalid without refresh token";
            ClearCookiesAndLog(cookieService, reason);
        }
    }

    private async Task HandleRefreshTokenOnlyScenarioAsync(
        IAuthenticationService authService, 
        ICookieService cookieService, 
        string refreshToken)
    {
        await TryRefreshOrClearCookiesAsync(authService, cookieService, refreshToken, "No access token, attempting refresh with refresh token only");
    }

    private async Task TryRefreshOrClearCookiesAsync(
        IAuthenticationService authService, 
        ICookieService cookieService, 
        string refreshToken, 
        string logContext)
    {
        var refreshSuccess = await TryRefreshTokenAsync(authService, cookieService, refreshToken);
        
        if (!refreshSuccess)
        {
            ClearCookiesAndLog(cookieService, $"{logContext} - refresh failed");
        }
    }

    private void ClearCookiesAndLog(ICookieService cookieService, string reason)
    {
        cookieService.ClearAuthCookies();
        _logger.LogWarning("Cookies cleared: {Reason}", reason);
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
                _logger.LogWarning("Token refresh failed: {Code}", refreshResult.Code);
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
