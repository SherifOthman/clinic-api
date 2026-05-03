using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Auth.Commands;
using ClinicManagement.Infrastructure.Options;
using ClinicManagement.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace ClinicManagement.API.Middleware;

/// <summary>
/// Reads the access token from the HttpOnly cookie and injects it as a
/// Bearer Authorization header so the existing JWT middleware works unchanged.
///
/// Silent refresh: if the access token is missing or expired and a refresh
/// token cookie is present, automatically calls RefreshTokenCommand and
/// rotates both cookies — the request continues without a round-trip to the client.
///
/// Debounce: concurrent requests (e.g. dashboard loading 5 calls at once) that
/// all arrive after token expiry are collapsed — only the first triggers a real
/// refresh; the rest wait and reuse the result via a short-lived in-memory lock.
/// </summary>
public class CookieTokenMiddleware
{
    private static readonly MemoryCache _refreshDebounce = new(new MemoryCacheOptions());
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, SemaphoreSlim>
        _refreshLocks = new();

    private static readonly JwtSecurityTokenHandler _jwtHandler = new();

    private readonly RequestDelegate _next;
    private readonly int _accessTokenExpiryMinutes;

    public CookieTokenMiddleware(RequestDelegate next, IOptions<JwtOptions> jwtOptions)
    {
        _next = next;
        _accessTokenExpiryMinutes = jwtOptions.Value.AccessTokenExpirationMinutes;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
        if (path.StartsWith("/api/auth/oauth", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var authHeader = context.Request.Headers.Authorization.ToString();
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            && authHeader.Length > "Bearer ".Length)
        {
            await _next(context);
            return;
        }

        var accessToken  = context.Request.Cookies[CookieConstants.AccessToken];
        var refreshToken = context.Request.Cookies[CookieConstants.RefreshToken];

        // Happy path — valid, non-expired token
        if (!string.IsNullOrEmpty(accessToken) && !IsTokenExpired(accessToken))
        {
            context.Request.Headers.Authorization = $"Bearer {accessToken}";
            await _next(context);
            return;
        }

        // Access token missing or expired — try silent refresh
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var newAccessToken = await TrySilentRefreshAsync(context, refreshToken);
            if (!string.IsNullOrEmpty(newAccessToken))
                context.Request.Headers.Authorization = $"Bearer {newAccessToken}";
        }

        await _next(context);
    }

    /// <summary>Returns true if the JWT is expired or cannot be read.</summary>
    private static bool IsTokenExpired(string token)
    {
        try
        {
            if (_jwtHandler.ReadToken(token) is not JwtSecurityToken jwt)
                return true;

            // 30-second buffer — refresh proactively before hard expiry
            return jwt.ValidTo < DateTime.UtcNow.AddSeconds(30);
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    /// Refreshes the token, with debouncing for concurrent requests.
    /// If multiple requests arrive simultaneously with the same expired token,
    /// only one hits the DB — the rest wait and reuse the result.
    /// </summary>
    private async Task<string?> TrySilentRefreshAsync(HttpContext context, string refreshToken)
    {
        // Use a short hash of the refresh token as the dedup key
        var lockKey = $"refresh:{refreshToken.GetHashCode()}";

        // Fast path — another request already refreshed, reuse the result
        if (_refreshDebounce.TryGetValue(lockKey, out string? cached))
            return cached;

        var semaphore = _refreshLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();
        try
        {
            // Double-check after acquiring the lock
            if (_refreshDebounce.TryGetValue(lockKey, out cached))
                return cached;

            var result = await ExecuteRefreshAsync(context, refreshToken);

            if (result is not null)
                // Cache the new token for 10 seconds to absorb concurrent burst
                _refreshDebounce.Set(lockKey, result, TimeSpan.FromSeconds(10));

            return result;
        }
        finally
        {
            semaphore.Release();
            _refreshLocks.TryRemove(lockKey, out _);
        }
    }

    private async Task<string?> ExecuteRefreshAsync(HttpContext context, string refreshToken)
    {
        try
        {
            var sender        = context.RequestServices.GetRequiredService<ISender>();
            var cookieService = context.RequestServices.GetRequiredService<ICookieService>();

            var result = await sender.Send(new RefreshTokenCommand(refreshToken));

            if (result.IsFailure || result.Value is null)
            {
                cookieService.ClearAllAuthCookies();
                return null;
            }

            cookieService.SetAccessTokenCookie(result.Value.AccessToken!, _accessTokenExpiryMinutes);
            cookieService.SetRefreshTokenCookie(result.Value.RefreshToken!);

            return result.Value.AccessToken;
        }
        catch
        {
            return null;
        }
    }
}
