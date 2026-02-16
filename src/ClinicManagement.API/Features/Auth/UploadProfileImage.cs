using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;
using ClinicManagement.API.Entities;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.API.Features.Auth;

public class UploadProfileImageEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/profile/image/upload", HandleAsync)
            .RequireAuthorization()
            .DisableAntiforgery()
            .WithName("UploadProfileImage")
            .WithSummary("Upload profile image")
            .WithTags("Authentication")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            ;
    }

    private static async Task<IResult> HandleAsync(
        IFormFile file,
        CurrentUserService currentUserService,
        UserManager<User> userManager,
        LocalFileStorageService fileStorageService,
        ApplicationDbContext db,
        CancellationToken ct)
    {
        // Validate file
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        const long maxFileSize = 5 * 1024 * 1024; // 5MB

        if (file == null || file.Length == 0)
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.REQUIRED_FIELD,
                Title = "File Required",
                Status = StatusCodes.Status400BadRequest,
                Detail = "File is required"
            });

        if (file.Length > maxFileSize)
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.INVALID_FORMAT,
                Title = "File Too Large",
                Status = StatusCodes.Status400BadRequest,
                Detail = "File size must not exceed 5MB"
            });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.INVALID_FORMAT,
                Title = "Invalid File Type",
                Status = StatusCodes.Status400BadRequest,
                Detail = $"File must be one of the following types: {string.Join(", ", allowedExtensions)}"
            });

        var user = await userManager.FindByIdAsync(currentUserService.UserId!.Value.ToString());
        if (user == null)
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.USER_NOT_FOUND,
                Title = "User Not Found",
                Status = StatusCodes.Status400BadRequest,
                Detail = "User not found"
            });

        try
        {
            // Delete old profile image if exists
            if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
            {
                await fileStorageService.DeleteFileAsync(user.ProfileImageUrl, ct);
            }

            // Upload new profile image
            var filePath = await fileStorageService.UploadFileAsync(file, "profiles", ct);

            // Update user profile image URL
            user.ProfileImageUrl = filePath;

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                // Rollback: delete uploaded file
                await fileStorageService.DeleteFileAsync(filePath, ct);

                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                return Results.BadRequest(new ApiProblemDetails
                {
                    Code = ErrorCodes.OPERATION_FAILED,
                    Title = "Update Failed",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = $"Failed to update profile: {errors}"
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
        catch (Exception ex)
        {
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.INTERNAL_ERROR,
                Title = "Upload Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = $"An error occurred while uploading profile image: {ex.Message}"
            });
        }
    }

    public record Request(IFormFile File);

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
