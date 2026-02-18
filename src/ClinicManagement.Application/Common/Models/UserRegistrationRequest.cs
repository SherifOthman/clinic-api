namespace ClinicManagement.Application.Common.Models;

public record UserRegistrationRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string Role, // ASP.NET Identity role name (e.g., "ClinicOwner", "Doctor")
    Guid? ClinicId = null, // Clinic membership (null for SuperAdmin)
    string? UserName = null,
    bool EmailConfirmed = false,
    bool SendConfirmationEmail = true
);
