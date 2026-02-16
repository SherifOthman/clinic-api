using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;
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
        var user = await userManager.FindByIdAsync(currentUserService.UserId!.Value.ToString());
        if (user == null)
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.USER_NOT_FOUND,
                Title = "User Not Found",
                Status = StatusCodes.Status400BadRequest,
                Detail = "User not found"
            });

        if (!await userManager.CheckPasswordAsync(user, request.CurrentPassword))
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.INVALID_CREDENTIALS,
                Title = "Invalid Password",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Current password is incorrect"
            });

        var result = await userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.OPERATION_FAILED,
                Title = "Password Change Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = errors
            });
        }

        return Results.Ok(new MessageResponse("Password changed successfully"));
    }

    public record Request(
        [Required]
        string CurrentPassword,
        
        [Required]
        [MinLength(6)]
        string NewPassword);
}
