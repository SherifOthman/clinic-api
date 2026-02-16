using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Extensions;
using ClinicManagement.API.Common.Models;
using ClinicManagement.API.Entities;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.API.Features.Auth;

public class ResetPasswordEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/reset-password", HandleAsync)
            .AllowAnonymous()
            .WithName("ResetPassword")
            .WithSummary("Reset password with token")
            .WithTags("Authentication")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            ;
    }

    private static async Task<IResult> HandleAsync(
        Request request,
        UserManager<User> userManager,
        CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.TOKEN_INVALID,
                Title = "Invalid Token",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Invalid reset token"
            });

        try
        {
            var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            result.ThrowIfFailed();

            return Results.Ok(new MessageResponse("Password reset successfully"));
        }
        catch (Exception ex)
        {
            return ex.HandleDomainException();
        }
    }

    public record Request(
        [Required]
        [EmailAddress]
        string Email,
        
        [Required]
        string Token,
        
        [Required]
        [MinLength(6)]
        string NewPassword);
}
