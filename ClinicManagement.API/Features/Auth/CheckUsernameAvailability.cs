using ClinicManagement.API.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Features.Auth;

/// <summary>
/// Real-time username availability check for registration forms
/// Called on blur/debounced typing for instant feedback
/// </summary>
public class CheckUsernameAvailabilityEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/auth/check-username", HandleAsync)
            .AllowAnonymous()
            .WithName("CheckUsernameAvailability")
            .WithSummary("Check if username is available for registration")
            .WithDescription("Returns whether the username is available. Used for real-time validation during registration.")
            .WithTags("Authentication")
            .Produces<Response>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        [FromQuery] string username,
        UserManager<User> userManager,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Results.Ok(new Response(false, "Username is required"));
        }

        var user = await userManager.FindByNameAsync(username);
        var isAvailable = user == null;

        return Results.Ok(new Response(
            isAvailable,
            isAvailable ? null : "Username is already taken"
        ));
    }

    public record Response(
        bool IsAvailable,
        string? Message);
}
