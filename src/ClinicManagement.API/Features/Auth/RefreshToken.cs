using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;
using System.Collections.Concurrent;

namespace ClinicManagement.API.Features.Auth;

public class RefreshTokenEndpoint : IEndpoint
{
    // Per-user semaphore to prevent concurrent refresh token requests
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _userLocks = new();

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/refresh", HandleAsync)
            .AllowAnonymous()
            .WithName("RefreshToken")
            .WithSummary("Refresh access token")
            .WithDescription("Refreshes access token using refresh token. Supports both web (cookie) and mobile (body) clients.")
            .WithTags("Authentication")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Request? request,
        AuthenticationService authService,
        CookieService cookieService,
        ILogger<RefreshTokenEndpoint> logger,
        CancellationToken ct)
    {
        // Determine client type and get refresh token
        var clientType = httpContext.Request.Headers["X-Client-Type"].ToString();
        var isMobile = clientType.Equals("mobile", StringComparison.OrdinalIgnoreCase);
        
        string? refreshToken;
        
        if (isMobile)
        {
            // Mobile: Get refresh token from request body
            refreshToken = request?.RefreshToken;
            logger.LogInformation("Mobile client refresh token request");
        }
        else
        {
            // Web: Get refresh token from HTTP-only cookie
            refreshToken = cookieService.GetRefreshTokenFromCookie();
            logger.LogInformation("Web client refresh token request");
        }

        if (string.IsNullOrEmpty(refreshToken))
        {
            logger.LogWarning("Refresh token missing");
            return Results.Unauthorized();
        }

        // Get user ID from refresh token to create per-user lock
        var userId = await GetUserIdFromRefreshTokenAsync(refreshToken, authService, ct);
        if (userId == null)
        {
            logger.LogWarning("Invalid refresh token");
            return Results.Unauthorized();
        }

        // Get or create semaphore for this user
        var semaphore = _userLocks.GetOrAdd(userId.Value, _ => new SemaphoreSlim(1, 1));
        
        await semaphore.WaitAsync(ct);
        try
        {
            // Refresh tokens
            var result = await authService.RefreshTokenAsync(refreshToken, ct);
            
            if (result == null)
            {
                logger.LogWarning("Token refresh failed for user {UserId}", userId);
                
                // Clear cookies for web clients
                if (!isMobile)
                {
                    cookieService.ClearAuthCookies();
                }
                
                return Results.Unauthorized();
            }

            logger.LogInformation("Token refreshed successfully for user {UserId}", userId);

            if (isMobile)
            {
                // Mobile: Return both tokens in response body
                return Results.Ok(new Response(result.AccessToken, result.RefreshToken));
            }
            else
            {
                // Web: Return access token in body, set refresh token in HTTP-only cookie
                cookieService.SetRefreshTokenCookie(result.RefreshToken);
                return Results.Ok(new Response(result.AccessToken, null));
            }
        }
        finally
        {
            semaphore.Release();
            
            // Cleanup: Remove semaphore if no one is waiting
            if (semaphore.CurrentCount == 1)
            {
                _userLocks.TryRemove(userId.Value, out _);
            }
        }
    }

    private static async Task<Guid?> GetUserIdFromRefreshTokenAsync(
        string refreshToken, 
        AuthenticationService authService,
        CancellationToken ct)
    {
        try
        {
            // We need to get the user ID from the refresh token
            // This requires accessing RefreshTokenService directly
            var refreshTokenService = authService.GetType()
                .GetField("_refreshTokenService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(authService) as RefreshTokenService;

            if (refreshTokenService == null) return null;

            var tokenEntity = await refreshTokenService.GetActiveRefreshTokenAsync(refreshToken, ct);
            return tokenEntity?.UserId;
        }
        catch
        {
            return null;
        }
    }

    public record Request(string? RefreshToken);

    public record Response(
        string AccessToken,
        string? RefreshToken); // Null for web clients (sent in cookie)
}
