namespace ClinicManagement.Application.Features.Auth.Queries.GetMe;

public record GetMeDto(
    Guid Id,
    string UserName,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? ProfileImageUrl,
    List<string> Roles,
    bool EmailConfirmed,
    bool OnboardingCompleted
);
