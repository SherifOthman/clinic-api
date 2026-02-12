using ClinicManagement.API.Common;
using ClinicManagement.API.Entities;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.API.Features.Auth;

public class ChangePasswordEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/change-password", HandleAsync)
            .RequireAuthorization()
            .WithName("ChangePassword")
            .WithSummary("Change current user password")
            .WithTags("Authentication")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            ;
    }

    private static async Task<IResult> HandleAsync(
        Request request,
        CurrentUserService currentUserService,
        UserManager<User> userManager,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == null)
            return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null)
            return Results.BadRequest(new { error = "User not found", code = "NOT_FOUND" });

        if (!await userManager.CheckPasswordAsync(user, request.CurrentPassword))
            return Results.BadRequest(new { error = "Current password is incorrect", code = "INVALID_PASSWORD" });

        var result = await userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Results.BadRequest(new { error = errors, code = "PASSWORD_CHANGE_FAILED" });
        }

        return Results.Ok(new { message = "Password changed successfully" });
    }

    public record Request(
        [Required]
        string CurrentPassword,
        
        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        string NewPassword);
}
