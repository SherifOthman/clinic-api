namespace ClinicManagement.Application.Features.Auth.QueryModels;

public record UserRoleRow(string RoleName);

/// <summary>
/// Single-query projection for GET /auth/me.
/// Replaces 7+ sequential queries with 2 parallel queries.
/// </summary>
public record GetMeProjection(
    string UserName,
    string FullName,
    string Email,
    string? PhoneNumber,
    string? ProfileImageUrl,
    bool EmailConfirmed,
    string Gender,
    bool HasPassword,
    bool OnboardingCompleted,
    // Member fields — null for clinic owners with no member record
    Guid? MemberId,
    Guid? DoctorInfoId,
    string? SpecializationNameEn,
    string? SpecializationNameAr,
    int WeekStartDay
);
