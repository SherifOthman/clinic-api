using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
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
        ProfileService profileService,
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

        var result = await profileService.UploadProfileImageAsync(user, file, ct);
        if (!result.IsSuccess)
        {
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = result.ErrorCode!,
                Title = "Upload Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = result.ErrorMessage!
            });
        }

        return Results.NoContent();
    }

    public record Request(IFormFile File);
}
