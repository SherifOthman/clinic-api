using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Options;
using ClinicManagement.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ClinicManagement.API.Features.Auth;

public class ForgotPasswordEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/forgot-password", HandleAsync)
            .AllowAnonymous()
            .WithName("ForgotPassword")
            .WithSummary("Request password reset")
            .WithTags("Authentication")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            ;
    }

    private static async Task<IResult> HandleAsync(
        Request request,
        UserManager<User> userManager,
        SmtpEmailSender emailSender,
        IOptions<SmtpOptions> smtpOptions,
        CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        // Always return success to prevent email enumeration
        if (user == null)
            return Results.Ok(new MessageResponse("Password reset email sent"));

        // Generate password reset token
        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        // Create reset link
        var resetLink = $"{smtpOptions.Value.FrontendUrl}/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

        // Send email
        var emailBody = EmailTemplates.GetPasswordResetTemplate($"{user.FirstName} {user.LastName}".Trim(), resetLink);

        try
        {
            await emailSender.SendEmailAsync(
                user.Email!,
                "Reset your password",
                emailBody,
                ct);
        }
        catch
        {
            // Still return success to prevent email enumeration
        }

        return Results.Ok(new MessageResponse("Password reset email sent"));
    }

    public record Request(
        [Required]
        [EmailAddress]
        string Email);
}
