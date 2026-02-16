using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;
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
                return Results.BadRequest(new ApiProblemDetails
                {
                    Code = ErrorCodes.USER_NOT_FOUND,
                    Title = "User Not Found",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "User not found"
                });

            if (await emailConfirmationService.IsEmailConfirmedAsync(user, ct))
                return Results.Ok(new MessageResponse("Email already confirmed"));

            await emailConfirmationService.ConfirmEmailAsync(user, request.Token, ct);
            return Results.Ok(new MessageResponse("Email confirmed successfully"));
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
