namespace ClinicManagement.API.Middleware;

/// <summary>
/// Reads the access token from the HttpOnly cookie and injects it as a
/// Bearer Authorization header so the existing JWT middleware works unchanged.
///
/// This runs before authentication — if the header is already set (mobile clients
/// sending the token manually) it is left untouched.
/// </summary>
public class CookieTokenMiddleware
{
    private const string AccessTokenCookie = "accessToken";
    private readonly RequestDelegate _next;

    public CookieTokenMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Only inject if no Authorization header is already present
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            var token = context.Request.Cookies[AccessTokenCookie];
            if (!string.IsNullOrEmpty(token))
                context.Request.Headers.Authorization = $"Bearer {token}";
        }

        await _next(context);
    }
}
