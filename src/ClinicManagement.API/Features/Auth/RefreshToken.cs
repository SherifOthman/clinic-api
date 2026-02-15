using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;
using System.Collections.Concurrent;

namespace ClinicManagement.API.Features.Auth;

/// <summary>
/// Refresh token endpoint supporting both web and mobile clients
/// Uses per-user semaphore to prevent concurrent refresh requests
/// </summary>
public class RefreshTokenEndpoint : IEndpoint
{
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
        RefreshTokenService refreshTokenService,
        AuthenticationService authService,
        CookieService cookieService,
        ILogger<RefreshTokenEndpoint> logger,
        CancellationToken ct)
    {
        try
        {
            var clientType = httpContext.Request.Headers["X-Client-Type"].ToString();
            var isMobile = clientType.Equals("mobile", StringComparison.OrdinalIgnoreCase);
            
            var refreshToken = isMobile 
                ? request?.RefreshToken 
                : cookieService.GetRefreshTokenFromCookie();

            if (string.IsNullOrEmpty(refreshToken))
            {
                logger.LogWarning("{ClientType} client refresh request with missing token", isMobile ? "Mobile" : "Web");
                return Results.Unauthorized();
            }

            var tokenEntity = await refreshTokenService.GetActiveRefreshTokenAsync(refreshToken, ct);
            if (tokenEntity == null)
            {
                logger.LogWarning("Invalid or expired refresh token");
                if (!isMobile) cookieService.ClearRefreshTokenCookie();
                return Results.Unauthorized();
            }

            var userId = tokenEntity.UserId;
            var semaphore = _userLocks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
            
            await semaphore.WaitAsync(ct);
            try
            {
                var result = await authService.RefreshTokenAsync(refreshToken, ct);
                
                if (result == null)
                {
                    logger.LogWarning("Token refresh failed for user {UserId}", userId);
                    if (!isMobile) cookieService.ClearRefreshTokenCookie();
                    return Results.Unauthorized();
                }

                logger.LogInformation("Token refreshed for user {UserId} ({ClientType})", 
                    userId, isMobile ? "mobile" : "web");

                if (isMobile)
                {
                    return Results.Ok(new Response(result.AccessToken, result.RefreshToken));
                }
                
                cookieService.SetRefreshTokenCookie(result.RefreshToken);
                return Results.Ok(new Response(result.AccessToken, null));
            }
            finally
            {
                semaphore.Release();
                
                if (semaphore.CurrentCount == 1)
                {
                    _userLocks.TryRemove(userId, out _);
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Refresh token request was cancelled");
            return Results.StatusCode(499); // Client Closed Request
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during token refresh");
            return Results.Problem("An error occurred while refreshing the token");
        }
    }

    public record Request(string? RefreshToken);

    public record Response(
        string AccessToken,
        string? RefreshToken);
}
