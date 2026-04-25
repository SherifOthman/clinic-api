namespace ClinicManagement.Application.Features.Staff.QueryModels;

public record StaffListRow(
    Guid Id, Guid UserId, bool IsActive, DateTimeOffset CreatedAt,
    string FullName, string Gender, string? ProfileImageUrl
);

public record StaffDetailRow(
    Guid Id, Guid UserId, bool IsActive, DateTimeOffset CreatedAt,
    string FullName, string Gender, string? Email, string? PhoneNumber,
    string? ProfileImageUrl, DoctorDetailRow? DoctorProfile
);

public record DoctorDetailRow(Guid Id, string SpecializationNameEn, string SpecializationNameAr, bool CanSelfManageSchedule);

public record StaffRoleRow(Guid UserId, string RoleName);

public record InvitationListRow(
    Guid Id, string Email, string Role,
    string? SpecializationNameEn, string? SpecializationNameAr,
    DateTimeOffset CreatedAt, DateTimeOffset ExpiresAt, bool IsAccepted, bool IsCanceled, string InvitedBy
);

public record InvitationDetailRow(
    Guid Id, string Email, string Role,
    string? SpecializationNameEn, string? SpecializationNameAr,
    DateTimeOffset CreatedAt, DateTimeOffset ExpiresAt, bool IsAccepted, bool IsCanceled,
    DateTimeOffset? AcceptedAt, string CreatedByName, string? AcceptedByName
);

public record WorkingDayRow(int Day, string StartTime, string EndTime, bool IsAvailable, Guid BranchId);
