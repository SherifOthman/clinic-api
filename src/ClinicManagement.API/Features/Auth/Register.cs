using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Enums;
using System.ComponentModel;

namespace ClinicManagement.API.Features.Auth;

public class RegisterEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", HandleAsync)
            .AllowAnonymous()
            .WithName("Register")
            .WithSummary("Register a new user")
            .WithDescription("Creates a new clinic owner account and sends email confirmation")
            .WithTags("Authentication")
            .Accepts<Request>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        Request request,
        UserRegistrationService userRegistrationService,
        CancellationToken ct)
    {
        var registrationRequest = new UserRegistrationRequest(
            Email: request.Email,
            Password: request.Password,
            FirstName: request.FirstName,
            LastName: request.LastName,
            PhoneNumber: request.PhoneNumber,
            UserType: UserType.ClinicOwner,
            UserName: request.UserName,
            EmailConfirmed: false,
            OnboardingCompleted: false,
            SendConfirmationEmail: true
        );

        try
        {
            await userRegistrationService.RegisterUserAsync(registrationRequest, ct);
            return Results.Ok(new { message = "Registration successful. Please check your email to confirm your account." });
        }
        catch (Exception ex)
        {
            return ex.HandleDomainException();
        }
    }

    public record Request(
        [Required]
        [MaxLength(100)]
        string FirstName,
        
        [Required]
        [MaxLength(100)]
        string LastName,
        
        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        string UserName,
        
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        string Email,
        
        [Required]
        [MinLength(6)]
        string Password,

        [Required]
        [MaxLength(20)]
        string PhoneNumber);
}
