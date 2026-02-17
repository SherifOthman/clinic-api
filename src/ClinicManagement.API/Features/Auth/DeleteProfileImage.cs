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
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
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

        var result = await profileService.DeleteProfileImageAsync(user, ct);
        if (!result.IsSuccess)
        {
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = result.ErrorCode!,
                Title = "Delete Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = result.ErrorMessage!
            });
        }

        return Results.NoContent();
    }
}
