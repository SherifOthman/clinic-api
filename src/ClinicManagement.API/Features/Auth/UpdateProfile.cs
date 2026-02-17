using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;
using ClinicManagement.API.Entities;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.API.Features.Auth;

public class UpdateProfileEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/auth/profile", HandleAsync)
            .RequireAuthorization()
            .WithName("UpdateProfile")
            .WithSummary("Update user profile")
            .WithTags("Authentication")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            ;
    }

    private static async Task<IResult> HandleAsync(
        Request request,
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

        var result = await profileService.UpdateProfileAsync(
            user,
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            ct);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = result.ErrorCode!,
                Title = "Update Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = result.ErrorMessage!
            });
        }

        return Results.NoContent();
    }

    public record Request(
        [Required]
        [MaxLength(50)]
        string FirstName,
        
        [Required]
        [MaxLength(50)]
        string LastName,
        
        [MaxLength(15)]
        string? PhoneNumber);
}
