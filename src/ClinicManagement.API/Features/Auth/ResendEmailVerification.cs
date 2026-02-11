using ClinicManagement.API.Common;
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
        UserManagementService userManagementService,
        EmailConfirmationService emailConfirmationService,
        CancellationToken ct)
    {
        var user = await userManagementService.GetUserByEmailAsync(request.Email, ct);
        if (user == null)
            return Results.BadRequest(new { error = "User not found", code = "NOT_FOUND" });

        if (await emailConfirmationService.IsEmailConfirmedAsync(user, ct))
            return Results.BadRequest(new { error = "Email is already confirmed", code = "EMAIL_ALREADY_CONFIRMED" });

        try
        {
            await emailConfirmationService.SendConfirmationEmailAsync(user, ct);
            return Results.Ok(new { message = "Verification email sent" });
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
