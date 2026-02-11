using ClinicManagement.API.Common;
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
        var userId = currentUserService.UserId;
        if (userId == null)
            return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null)
            return Results.BadRequest(new { error = "User not found", code = "NOT_FOUND" });

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
            return Results.BadRequest(new { error = $"Failed to update profile: {errors}", code = "UPDATE_FAILED" });
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
        [MaxLength(100, ErrorMessage = "First name must not exceed 100 characters")]
        string FirstName,
        
        [Required]
        [MaxLength(100, ErrorMessage = "Last name must not exceed 100 characters")]
        string LastName,
        
        [MaxLength(20, ErrorMessage = "Phone number must not exceed 20 characters")]
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
