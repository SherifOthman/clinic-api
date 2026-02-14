using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;
using ClinicManagement.API.Entities;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.API.Features.Auth;

public class LoginEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", HandleAsync)
            .AllowAnonymous()
            .WithName("Login")
            .WithSummary("Login with email and password")
            .WithDescription("Authenticates user and returns access token. Supports both web (cookie) and mobile (body) clients.")
            .WithTags("Authentication")
            .Accepts<Request>("application/json")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        Request request,
        HttpContext httpContext,
        ApplicationDbContext db,
        UserManager<User> userManager,
        TokenService tokenService,
        RefreshTokenService refreshTokenService,
        CookieService cookieService,
        ILogger<LoginEndpoint> logger,
        CancellationToken ct)
    {
        // Determine client type
        var clientType = httpContext.Request.Headers["X-Client-Type"].ToString();
        var isMobile = clientType.Equals("mobile", StringComparison.OrdinalIgnoreCase);
        
        logger.LogInformation("Login attempt for {Email} from {ClientType} client", request.Email, isMobile ? "mobile" : "web");
   
        // Find user by email
        var user = await userManager.FindByEmailAsync(request.Email);

        // Check password
        if (user == null || !(await userManager.CheckPasswordAsync(user, request.Password)))
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.INVALID_CREDENTIALS,
                Title = "Invalid Credentials",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Invalid email or password"
            });

        // Check if email is confirmed
        if (!user.EmailConfirmed)
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.EMAIL_NOT_CONFIRMED,
                Title = "Email Not Confirmed",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Please confirm your email before logging in"
            });

        // Get user roles
        var roles = await userManager.GetRolesAsync(user);

        // Generate tokens
        var accessToken = tokenService.GenerateAccessToken(user, roles);
        var refreshToken = await refreshTokenService.GenerateRefreshTokenAsync(user.Id, null, ct);

        logger.LogInformation("User {UserId} logged in successfully", user.Id);

        if (isMobile)
        {
            // Mobile: Return both tokens in response body
            return Results.Ok(new Response(accessToken, refreshToken.Token));
        }
        else
        {
            // Web: Return access token in body, set refresh token in HTTP-only cookie
            cookieService.SetRefreshTokenCookie(refreshToken.Token);
            return Results.Ok(new Response(accessToken, null));
        }
    }

    public record Request(
        [Required]
        [EmailAddress]
        string Email,
        
        [Required]
        string Password);

    public record Response(
        string AccessToken,
        string? RefreshToken); // Null for web clients (sent in cookie)
}
