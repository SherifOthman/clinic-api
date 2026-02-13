using ClinicManagement.API.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Features.Auth;

/// <summary>
/// Real-time email availability check for registration forms
/// Called on blur/debounced typing for instant feedback
/// </summary>
public class CheckEmailAvailabilityEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/auth/check-email", HandleAsync)
            .AllowAnonymous()
            .WithName("CheckEmailAvailability")
            .WithSummary("Check if email is available for registration")
            .WithDescription("Returns whether the email is available. Used for real-time validation during registration.")
            .WithTags("Authentication")
            .Produces<Response>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        [FromQuery] string email,
        UserManager<User> userManager,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Results.Ok(new Response(false, "Email is required"));
        }

        var user = await userManager.FindByEmailAsync(email);
        var isAvailable = user == null;

        return Results.Ok(new Response(
            isAvailable,
            isAvailable ? null : "Email is already taken"
        ));
    }

        public record Response(
            bool IsAvailable,
            string? Message);
}
