using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;
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
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(currentUserService.UserId!.Value.ToString());
        if (user == null)
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.USER_NOT_FOUND,
                Title = "User Not Found",
                Status = StatusCodes.Status400BadRequest,
                Detail = "User not found"
            });

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
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.OPERATION_FAILED,
                Title = "Delete Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = $"Failed to delete profile image: {errors}"
            });
        }

        var roles = await userManager.GetRolesAsync(user);

        // Check if user has completed onboarding
        var hasClinic = await db.Clinics
            .AnyAsync(c => c.OwnerUserId == user.Id && c.OnboardingCompleted, ct);

        var response = new Response(
            user.Id,
            user.Email!,
            user.UserName!,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.ProfileImageUrl,
            roles.ToList(),
            user.EmailConfirmed,
            hasClinic
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
        List<string> Roles,
        bool EmailConfirmed,
        bool OnboardingCompleted);
}
