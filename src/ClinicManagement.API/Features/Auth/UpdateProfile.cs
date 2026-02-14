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
            .Produces<Response>(StatusCodes.Status200OK)
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

        // Update user properties
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
            ? null
            : request.PhoneNumber.Trim();

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.OPERATION_FAILED,
                Title = "Update Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = $"Failed to update profile: {errors}"
            });
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

    public record Request(
        [Required]
        [MaxLength(100)]
        string FirstName,
        
        [Required]
        [MaxLength(100)]
        string LastName,
        
        [MaxLength(20)]
        string? PhoneNumber);

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
