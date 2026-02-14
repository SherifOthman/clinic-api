using Microsoft.AspNetCore.Authorization;

namespace ClinicManagement.API.Infrastructure.Middleware;

public class JwtCookieMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtCookieMiddleware> _logger;
    private static readonly SemaphoreSlim _refreshSemaphore = new SemaphoreSlim(1, 1);
    private static readonly Dictionary<string, (string AccessToken, string RefreshToken, DateTime Expiry)> _tokenCache = new();
    private static readonly object _cacheLock = new object();
    private const int TokenCacheExpirySeconds = 5; // Cache tokens for 5 seconds to handle concurrent requests

    public JwtCookieMiddleware(RequestDelegate next, ILogger<JwtCookieMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, TokenService tokenService, AuthenticationService authService, CookieService cookieService)
    {
        if (ShouldSkipTokenValidation(context))
        {
            await _next(context);
            return;
        }

        var accessToken = cookieService.GetAccessTokenFromCookie();
        var refreshToken = cookieService.GetRefreshTokenFromCookie();

        await HandleTokenValidationAsync(context, tokenService, authService, cookieService, accessToken, refreshToken);

        await _next(context);
    }

    private bool ShouldSkipTokenValidation(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        return endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null;
    }

    private async Task HandleTokenValidationAsync(
        HttpContext context,
        TokenService tokenService, 
        AuthenticationService authService, 
        CookieService cookieService, 
        string? accessToken, 
        string? refreshToken)
    {
        if (HasAccessToken(accessToken))
        {
            await HandleAccessTokenScenarioAsync(context, tokenService, authService, cookieService, accessToken!, refreshToken);
        }
        else if (HasRefreshToken(refreshToken))
        {
            await HandleRefreshTokenOnlyScenarioAsync(context, authService, cookieService, refreshToken!);
        }
        // If no tokens at all, let the request continue and JWT Bearer will return 401
    }

    private bool HasAccessToken(string? token) => !string.IsNullOrEmpty(token);
    
    private bool HasRefreshToken(string? token) => !string.IsNullOrEmpty(token);

    private async Task HandleAccessTokenScenarioAsync(
        HttpContext context,
        TokenService tokenService, 
        AuthenticationService authService, 
        CookieService cookieService, 
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
            await TryRefreshOrClearCookiesAsync(context, authService, cookieService, refreshToken!, logMessage);
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
        HttpContext context,
        AuthenticationService authService, 
        CookieService cookieService, 
        string refreshToken)
    {
        await TryRefreshOrClearCookiesAsync(context, authService, cookieService, refreshToken, "No access token, attempting refresh with refresh token only");
    }

    private async Task TryRefreshOrClearCookiesAsync(
        HttpContext context,
        AuthenticationService authService, 
        CookieService cookieService, 
        string refreshToken, 
        string logContext)
    {
        var refreshSuccess = await TryRefreshTokenAsync(authService, cookieService, refreshToken, context);
        
        if (!refreshSuccess)
        {
            ClearCookiesAndLog(cookieService, $"{logContext} - refresh failed");
        }
    }

    private void ClearCookiesAndLog(CookieService cookieService, string reason)
    {
        cookieService.ClearAuthCookies();
        _logger.LogWarning("Cookies cleared: {Reason}", reason);
    }

    private async Task<bool> TryRefreshTokenAsync(AuthenticationService authService, CookieService cookieService, string refreshToken, HttpContext context)
    {
        // Use semaphore to prevent race condition when multiple requests try to refresh simultaneously
        await _refreshSemaphore.WaitAsync();
        
        try
        {
            // Check if we have a cached refreshed token for this refresh token
            lock (_cacheLock)
            {
                if (_tokenCache.TryGetValue(refreshToken, out var cachedTokens))
                {
                    if (cachedTokens.Expiry > DateTime.UtcNow)
                    {
                        // Use cached tokens
                        cookieService.SetAccessTokenCookie(cachedTokens.AccessToken);
                        cookieService.SetRefreshTokenCookie(cachedTokens.RefreshToken);
                        context.Request.Headers["Authorization"] = $"Bearer {cachedTokens.AccessToken}";
                        _logger.LogInformation("Using cached refreshed tokens");
                        return true;
                    }
                    else
                    {
                        // Cached tokens expired, remove from cache
                        _tokenCache.Remove(refreshToken);
                    }
                }
            }
            
            // Double-check if token is still valid after acquiring lock
            // Another request might have already refreshed it
            var currentAccessToken = cookieService.GetAccessTokenFromCookie();
            if (!string.IsNullOrEmpty(currentAccessToken))
            {
                var validationResult = context.RequestServices.GetRequiredService<TokenService>()
                    .ValidateAccessTokenWithExpiry(currentAccessToken);
                
                if (validationResult.IsValid)
                {
                    // Token was refreshed by another request, use it
                    context.Request.Headers["Authorization"] = $"Bearer {currentAccessToken}";
                    _logger.LogInformation("Token already refreshed by another request");
                    return true;
                }
            }
            
            // Proceed with refresh
            var refreshResult = await authService.RefreshTokenAsync(refreshToken);
            
            if (refreshResult != null)
            {
                cookieService.SetAccessTokenCookie(refreshResult.AccessToken);
                cookieService.SetRefreshTokenCookie(refreshResult.RefreshToken);
                
                // Cache the new tokens for concurrent request handling
                lock (_cacheLock)
                {
                    _tokenCache[refreshToken] = (refreshResult.AccessToken, refreshResult.RefreshToken, DateTime.UtcNow.AddSeconds(TokenCacheExpirySeconds));
                    
                    // Cleanup old cache entries
                    var expiredKeys = _tokenCache.Where(kvp => kvp.Value.Expiry <= DateTime.UtcNow).Select(kvp => kvp.Key).ToList();
                    foreach (var key in expiredKeys)
                    {
                        _tokenCache.Remove(key);
                    }
                }
                
                // Update the Authorization header for the current request
                context.Request.Headers["Authorization"] = $"Bearer {refreshResult.AccessToken}";
                
                _logger.LogInformation("Access token refreshed successfully");
                return true;
            }
            else
            {
                _logger.LogWarning("Token refresh failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return false;
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }
}
