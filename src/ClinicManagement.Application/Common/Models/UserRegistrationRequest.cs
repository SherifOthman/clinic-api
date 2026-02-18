namespace ClinicManagement.Application.Common.Models;

public record UserRegistrationRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string Role,
    int? ClinicId = null,
    string? UserName = null,
    bool EmailConfirmed = false,
    bool SendConfirmationEmail = true
);
