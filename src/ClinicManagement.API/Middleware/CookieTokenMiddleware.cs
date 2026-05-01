using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Auth.Commands;
using MediatR;

namespace ClinicManagement.API.Middleware;

/// <summary>
/// Reads the access token from the HttpOnly cookie and injects it as a
/// Bearer Authorization header so the existing JWT middleware works unchanged.
///
/// Silent refresh: if the access token is missing or expired and a refresh
/// token cookie is present, automatically calls RefreshTokenCommand and
/// rotates both cookies — the request continues without a round-trip to the client.
/// </summary>
public class CookieTokenMiddleware
{
    private const string AccessTokenCookie  = "accessToken";
    private const string RefreshTokenCookie = "refreshToken";

    private readonly RequestDelegate _next;

    public CookieTokenMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip OAuth callback paths — the Google handler needs its own cookies untouched
        var path = context.Request.Path.Value ?? "";
        if (path.StartsWith("/api/auth/oauth", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Mobile clients send Authorization header themselves — leave untouched.
        // Only skip if the header contains an actual Bearer token value, not just
        // an empty/whitespace header that tools like Scalar inject automatically.
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            && authHeader.Length > "Bearer ".Length)
        {
            await _next(context);
            return;
        }

        var accessToken = context.Request.Cookies[AccessTokenCookie];

        if (!string.IsNullOrEmpty(accessToken))
        {
            // Happy path — inject existing access token
            context.Request.Headers.Authorization = $"Bearer {accessToken}";
            await _next(context);
            return;
        }

        // No access token — try silent refresh if refresh token cookie exists
        var refreshToken = context.Request.Cookies[RefreshTokenCookie];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var newAccessToken = await TrySilentRefreshAsync(context, refreshToken);
            if (!string.IsNullOrEmpty(newAccessToken))
                context.Request.Headers.Authorization = $"Bearer {newAccessToken}";
            // If refresh failed, cookies are cleared and request continues unauthenticated → 401
        }

        await _next(context);
    }

    private static async Task<string?> TrySilentRefreshAsync(HttpContext context, string refreshToken)
    {
        try
        {
            var sender      = context.RequestServices.GetRequiredService<ISender>();
            var cookieService = context.RequestServices.GetRequiredService<ICookieService>();

            var result = await sender.Send(new RefreshTokenCommand(refreshToken, IsMobile: false));

            if (result.IsFailure || result.Value is null)
            {
                // Refresh token invalid/expired — clear stale cookies
                cookieService.ClearAllAuthCookies();
                return null;
            }

            // Rotate both cookies
            cookieService.SetAccessTokenCookie(result.Value.AccessToken!, 60);
            cookieService.SetRefreshTokenCookie(result.Value.RefreshToken!);

            return result.Value.AccessToken;
        }
        catch
        {
            // Never let a refresh failure crash the request pipeline
            return null;
        }
    }
}
