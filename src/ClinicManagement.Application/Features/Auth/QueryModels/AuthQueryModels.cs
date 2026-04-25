namespace ClinicManagement.Application.Features.Auth.QueryModels;

public record UserRoleRow(string RoleName);

public record UserSpecializationRow(string NameEn, string NameAr);

public record UserProfileRow(
    string UserName,
    string FullName,
    string Email,
    string? PhoneNumber,
    string? ProfileImageUrl,
    bool EmailConfirmed,
    string Gender
);
