namespace ClinicManagement.Application.Features.Auth.QueryModels;

public record UserRoleRow(string RoleName);

public record UserSpecializationRow(string NameEn, string NameAr);

public record UserProfileRow(
    string UserName,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? ProfileImageUrl,
    bool EmailConfirmed,
    string Gender
);
