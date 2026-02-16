using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;
using ClinicManagement.API.Entities;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.API.Features.Auth;

public class LoginEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", HandleAsync)
            .AllowAnonymous()
            .WithName("Login")
            .WithSummary("Login with email/username and password")
            .WithDescription("Authenticates user with email or username and returns access token. Supports both web (cookie) and mobile (body) clients.")
            .WithTags("Authentication")
            .Accepts<Request>("application/json")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        Request request,
        HttpContext httpContext,
        UserManager<User> userManager,
        TokenService tokenService,
        RefreshTokenService refreshTokenService,
        CookieService cookieService,
        ApplicationDbContext db,
        ILogger<LoginEndpoint> logger,
        CancellationToken ct)
    {
        var clientType = httpContext.Request.Headers["X-Client-Type"].ToString();
        var isMobile = clientType.Equals("mobile", StringComparison.OrdinalIgnoreCase);
        
        // Try to find user by email first, then by username
        var user = await userManager.FindByEmailAsync(request.EmailOrUsername);
        if (user == null)
        {
            user = await userManager.FindByNameAsync(request.EmailOrUsername);
        }
        
        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            logger.LogWarning("Failed login attempt for {EmailOrUsername}", request.EmailOrUsername);
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.INVALID_CREDENTIALS,
                Title = "Invalid Credentials",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Invalid email/username or password"
            });
        }

        // Note: We allow login even if email is not confirmed
        // Frontend will handle redirecting to email verification page
        // This provides better UX and allows users to resend verification email
        if (!user.EmailConfirmed)
        {
            logger.LogInformation("User {UserId} logged in with unconfirmed email", user.Id);
        }

        var roles = await userManager.GetRolesAsync(user);
        
        // Get ClinicId from Staff table (if user is clinic staff)
        // SuperAdmin has no Staff record, so ClinicId will be null
        var staff = await db.Staff
            .Where(s => s.UserId == user.Id && s.IsActive)
            .FirstOrDefaultAsync(ct);
        
        var clinicId = staff?.ClinicId;
        
        var accessToken = tokenService.GenerateAccessToken(user, roles, clinicId);
        var refreshToken = await refreshTokenService.GenerateRefreshTokenAsync(user.Id, null, ct);

        logger.LogInformation("User {UserId} logged in ({ClientType}) with roles [{Roles}]", 
            user.Id, isMobile ? "mobile" : "web", string.Join(", ", roles));

        if (isMobile)
        {   
            return Results.Ok(new Response(accessToken, refreshToken.Token));
        }
        
        cookieService.SetRefreshTokenCookie(refreshToken.Token);
        return Results.Ok(new Response(accessToken, null));
    }

    public record Request(
        [Required]
        string EmailOrUsername,
        
        [Required]
        string Password);

    public record Response(
        string AccessToken,
        string? RefreshToken);
}
