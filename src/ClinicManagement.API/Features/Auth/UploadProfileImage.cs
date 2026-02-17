using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;
using ClinicManagement.API.Common.Options;

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
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            ;
    }

    private static async Task<IResult> HandleAsync(
        IFormFile file,
        CurrentUserService currentUserService,
        UserManager<User> userManager,
        LocalFileStorageService fileStorageService,
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

        try
        {
            // Delete old profile image if exists
            if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
            {
                await fileStorageService.DeleteFileAsync(user.ProfileImageUrl, ct);
            }

            // Upload and validate using file storage service
            var filePath = await fileStorageService.UploadFileWithValidationAsync(file, FileTypes.ProfileImage, ct);

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
                    Title = "Upload Failed",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = $"Failed to update profile: {errors}"
                });
            }

            return Results.NoContent();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.INVALID_FORMAT,
                Title = "Validation Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message
            });
        }
    }

    public record Request(IFormFile File);
}
