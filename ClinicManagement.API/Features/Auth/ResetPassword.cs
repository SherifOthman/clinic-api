using ClinicManagement.API.Common;
namespace ClinicManagement.API.Features.Auth;

public class ResetPasswordEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/reset-password", HandleAsync)
            .AllowAnonymous()
            .WithName("ResetPassword")
            .WithSummary("Reset password with token")
            .WithTags("Authentication")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            ;
    }

    private static async Task<IResult> HandleAsync(
        Request request,
        UserManagementService userManagementService,
        CancellationToken ct)
    {
        var user = await userManagementService.GetUserByEmailAsync(request.Email, ct);
        if (user == null)
            return Results.BadRequest(new { error = "Invalid reset token", code = "INVALID_TOKEN" });

        try
        {
            await userManagementService.ResetPasswordAsync(
                user,
                request.Token,
                request.NewPassword,
                ct);

            return Results.Ok(new { message = "Password reset successfully" });
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
        string Token,
        
        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        string NewPassword);
}
