using ClinicManagement.API.Common;
using ClinicManagement.API.Entities;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.API.Features.Auth;

public class ConfirmEmailEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/confirm-email", HandleAsync)
            .AllowAnonymous()
            .WithName("ConfirmEmail")
            .WithSummary("Confirm user email")
            .WithTags("Authentication")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            ;
    }

    private static async Task<IResult> HandleAsync(
        Request request,
        UserManager<User> userManager,
        EmailConfirmationService emailConfirmationService,
        CancellationToken ct)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Results.BadRequest(new { error = "User not found", code = "NOT_FOUND" });

            if (await emailConfirmationService.IsEmailConfirmedAsync(user, ct))
                return Results.Ok(new { message = "Email already confirmed" });

            await emailConfirmationService.ConfirmEmailAsync(user, request.Token, ct);
            return Results.Ok(new { message = "Email confirmed successfully" });
        }
        catch (Exception ex)
        {
            return ex.HandleDomainException();
        }
    }

    public record Request(
        [Required]
        [EmailAddress]
        string Email,
        
        [Required]
        string Token);
}
