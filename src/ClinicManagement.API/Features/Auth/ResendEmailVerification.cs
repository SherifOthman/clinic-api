using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;
using ClinicManagement.API.Entities;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.API.Features.Auth;

public class ResendEmailVerificationEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/resend-email-verification", HandleAsync)
            .AllowAnonymous()
            .WithName("ResendEmailVerification")
            .WithSummary("Resend email verification")
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
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.EMAIL_ALREADY_CONFIRMED,
                Title = "Email Already Confirmed",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Email is already confirmed"
            });

        try
        {
            await emailConfirmationService.SendConfirmationEmailAsync(user, ct);
            return Results.Ok(new MessageResponse("Verification email sent"));
        }
        catch (Exception ex)
        {
            return ex.HandleDomainException();
        }
    }

    public record Request(
        [Required]
        [EmailAddress]
        string Email);
}
