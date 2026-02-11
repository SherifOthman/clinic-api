using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Options;
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
        UserManagementService userManagementService,
        SmtpEmailSender emailSender,
        IOptions<SmtpOptions> smtpOptions,
        CancellationToken ct)
    {
        var user = await userManagementService.GetUserByEmailAsync(request.Email, ct);

        // Always return success to prevent email enumeration
        if (user == null)
            return Results.Ok(new { message = "Password reset email sent" });

        // Generate password reset token
        var token = await userManagementService.GeneratePasswordResetTokenAsync(user, ct);

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

        return Results.Ok(new { message = "Password reset email sent" });
    }

    public record Request(
        [Required]
        [EmailAddress]
        string Email);
}
