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
            .WithDescription("Authenticates user and returns access token with refresh token in cookie")
            .WithTags("Authentication")
            .Accepts<Request>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        Request request,
        ApplicationDbContext db,
        UserManager<User> userManager,
        TokenService tokenService,
        RefreshTokenService refreshTokenService,
        CookieService cookieService,
        CancellationToken ct)
    {
   
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
        var role = roles.FirstOrDefault() ?? "User";

        // Generate tokens
        var accessToken = tokenService.GenerateAccessToken(user, roles);
        var refreshToken = await refreshTokenService.GenerateRefreshTokenAsync(user.Id, null, ct);

        // Set tokens in cookies
        cookieService.SetAccessTokenCookie(accessToken);
        cookieService.SetRefreshTokenCookie(refreshToken.Token);


        return Results.NoContent();
    }

    public record Request(
        [Required]
        [EmailAddress]
        string Email,
        
        [Required]
        string Password);


}
