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
        AuthenticationService authenticationService,
        CookieService cookieService,
        CancellationToken ct)
    {
        // Get refresh token from cookie before clearing
        var refreshToken = cookieService.GetRefreshTokenFromCookie();

        // Delegate logout logic to service
        await authenticationService.LogoutAsync(refreshToken, ct);

        // Clear authentication cookies
        cookieService.ClearAuthCookies();

        return Results.Ok(new { message = "Logged out successfully" });
    }
}
