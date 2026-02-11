using ClinicManagement.API.Common;
using ClinicManagement.API.Entities;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.API.Features.Auth;

public class DeleteProfileImageEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete("/auth/profile/image", HandleAsync)
            .RequireAuthorization()
            .WithName("DeleteProfileImage")
            .WithSummary("Delete profile image")
            .WithTags("Authentication")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        CurrentUserService currentUserService,
        UserManager<User> userManager,
        LocalFileStorageService fileStorageService,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == null)
            return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null)
            return Results.BadRequest(new { error = "User not found", code = "NOT_FOUND" });

        // Delete profile image file if exists
        if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
        {
            await fileStorageService.DeleteFileAsync(user.ProfileImageUrl, ct);
        }

        // Clear profile image URL
        user.ProfileImageUrl = null;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            return Results.BadRequest(new { error = $"Failed to delete profile image: {errors}", code = "UPDATE_FAILED" });
        }

        var response = new Response(
            user.Id,
            user.Email!,
            user.UserName!,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.ProfileImageUrl,
            user.ClinicId,
            user.EmailConfirmed,
            user.OnboardingCompleted
        );

        return Results.Ok(response);
    }

    public record Response(
        Guid Id,
        string Email,
        string UserName,
        string FirstName,
        string LastName,
        string? PhoneNumber,
        string? ProfileImageUrl,
        Guid? ClinicId,
        bool EmailConfirmed,
        bool OnboardingCompleted);
}
