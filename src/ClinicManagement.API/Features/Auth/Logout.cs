using ClinicManagement.API.Common;

namespace ClinicManagement.API.Features.Auth;

public class LogoutEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/logout", HandleAsync)
            .RequireAuthorization()
            .WithName("Logout")
            .WithSummary("Logout current user")
            .WithTags("Authentication")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Request? request,
        AuthenticationService authenticationService,
        CookieService cookieService,
        ILogger<LogoutEndpoint> logger,
        CancellationToken ct)
    {
        // Determine client type
        var clientType = httpContext.Request.Headers["X-Client-Type"].ToString();
        var isMobile = clientType.Equals("mobile", StringComparison.OrdinalIgnoreCase);
        
        string? refreshToken;
        
        if (isMobile)
        {
            // Mobile: Get refresh token from request body
            refreshToken = request?.RefreshToken;
            logger.LogInformation("Mobile client logout");
        }
        else
        {
            // Web: Get refresh token from HTTP-only cookie
            refreshToken = cookieService.GetRefreshTokenFromCookie();
            logger.LogInformation("Web client logout");
        }

        // Revoke refresh token
        await authenticationService.LogoutAsync(refreshToken, ct);

        // Clear authentication cookies for web clients
        if (!isMobile)
        {
            cookieService.ClearAuthCookies();
        }

        return Results.Ok(new MessageResponse("Logged out successfully"));
    }

    public record Request(string? RefreshToken);
}
