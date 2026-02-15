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
        var clientType = httpContext.Request.Headers["X-Client-Type"].ToString();
        var isMobile = clientType.Equals("mobile", StringComparison.OrdinalIgnoreCase);
        
        string? refreshToken = isMobile 
            ? request?.RefreshToken 
            : cookieService.GetRefreshTokenFromCookie();

        await authenticationService.LogoutAsync(refreshToken, ct);

        if (!isMobile)
        {
            cookieService.ClearRefreshTokenCookie();
        }

        logger.LogInformation("{ClientType} client logged out", isMobile ? "Mobile" : "Web");
        return Results.Ok(new MessageResponse("Logged out successfully"));
    }

    public record Request(string? RefreshToken);
}
