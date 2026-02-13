using ClinicManagement.API.Common;
using ClinicManagement.API.Entities;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.API.Features.Auth;

public class GetMeEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/auth/me", HandleAsync)
            .RequireAuthorization()
            .WithName("GetMe")
            .WithSummary("Get current user information")
            .WithTags("Authentication")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        CurrentUserService currentUser,
        UserManager<User> userManager,
        CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (userId == null)
            return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null)
            return Results.Unauthorized();

        var roles = await userManager.GetRolesAsync(user);

        var response = new Response(
            user.Id,
            user.UserName!,
            user.FirstName,
            user.LastName,
            user.Email!,
            user.PhoneNumber,
            user.ProfileImageUrl,
            user.ProfileImageUpdatedAt,
            roles.ToList(),
            user.EmailConfirmed,
            user.IsActive,
            user.OnboardingCompleted,
            user.ClinicName,
            user.CreatedAt,
            user.UpdatedAt
        );

        return Results.Ok(response);
    }

    public record Response(
        Guid Id,
        string UserName,
        string FirstName,
        string LastName,
        string Email,
        string? PhoneNumber,
        string? ProfileImageUrl,
        DateTime? ProfileImageUpdatedAt,
        List<string> Roles,
        bool EmailConfirmed,
        bool IsActive,
        bool? OnboardingCompleted,
        string? ClinicName,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}
