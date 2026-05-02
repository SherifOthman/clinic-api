namespace ClinicManagement.Application.Features.Auth.QueryModels;

public record UserRoleRow(string RoleName);

/// <summary>
/// User + roles loaded in a single query — used by LoginHandler to avoid
/// a separate GetRolesAsync call after credential validation.
/// </summary>
public record UserWithRoles(
    Domain.Entities.User User,
    List<string> Roles
);

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
    int WeekStartDay,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset? LastPasswordChangeAt
);
