namespace ClinicManagement.Application.Abstractions.Services;

/// <summary>
/// Centralizes all "can the current user do X?" checks.
/// Handlers inject this instead of doing inline role/ownership checks.
///
/// Rules:
///   - ClinicOwner can do everything within their clinic.
///   - Doctor can manage their own schedule/visit types IF CanSelfManageSchedule = true.
///   - Adding a new role means updating this service only — not every handler.
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Can the current user manage the schedule (working days) of the given staff member?
    /// Owner: always. Doctor: only own + CanSelfManageSchedule = true.
    /// </summary>
    Task<SchedulePermissionResult> CanManageScheduleAsync(Guid targetStaffId, CancellationToken ct = default);

    /// <summary>
    /// Can the current user manage visit types for the given staff member?
    /// Same rules as schedule management.
    /// </summary>
    Task<SchedulePermissionResult> CanManageVisitTypesAsync(Guid targetStaffId, CancellationToken ct = default);

    /// <summary>
    /// Can the current user manage visit types for a specific doctor profile (by doctor ID, not staff ID)?
    /// Used when we already have the doctor profile ID (e.g. RemoveDoctorVisitType).
    /// </summary>
    Task<SchedulePermissionResult> CanManageVisitTypesByDoctorIdAsync(Guid doctorProfileId, CancellationToken ct = default);
}

/// <summary>Result of a schedule permission check.</summary>
public sealed class SchedulePermissionResult
{
    public bool IsAllowed { get; private init; }
    public string? DeniedReason { get; private init; }

    public static SchedulePermissionResult Allow() => new() { IsAllowed = true };

    public static SchedulePermissionResult Deny(string reason) =>
        new() { IsAllowed = false, DeniedReason = reason };
}
