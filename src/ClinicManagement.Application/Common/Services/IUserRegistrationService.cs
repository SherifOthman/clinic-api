using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Application.Common.Services;

public interface IUserRegistrationService
{
    Task<Result<Guid>> RegisterUserAsync(
        UserRegistrationRequest request, 
        CancellationToken cancellationToken = default);
}

public class UserRegistrationRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }
    public UserType UserType { get; set; }
    public Guid? ClinicId { get; set; }
    public bool EmailConfirmed { get; set; } = false;
    public bool OnboardingCompleted { get; set; } = false;
    public bool SendConfirmationEmail { get; set; } = true;
}
