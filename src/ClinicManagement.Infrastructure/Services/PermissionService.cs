using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Single place for all "can user X do action Y?" logic.
/// To add a new role: update the checks here only.
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _uow;

    public PermissionService(ICurrentUserService currentUser, IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _uow         = uow;
    }

    public async Task<SchedulePermissionResult> CanManageScheduleAsync(
        Guid targetStaffId, CancellationToken ct = default)
    {
        // Clinic owner can manage any doctor's schedule
        if (IsClinicOwner()) return SchedulePermissionResult.Allow();

        // Doctor can only manage their own schedule
        var currentStaff = await GetCurrentStaffAsync(ct);
        if (currentStaff?.Id != targetStaffId)
            return SchedulePermissionResult.Deny("You can only manage your own schedule");

        // Check if self-management is locked
        var doctorId = await _uow.DoctorProfiles.GetIdByStaffIdAsync(targetStaffId, ct);
        return await CheckSelfManageLockAsync(doctorId, ct);
    }

    public async Task<SchedulePermissionResult> CanManageVisitTypesAsync(
        Guid targetStaffId, CancellationToken ct = default)
    {
        if (IsClinicOwner()) return SchedulePermissionResult.Allow();

        var currentStaff = await GetCurrentStaffAsync(ct);
        if (currentStaff?.Id != targetStaffId)
            return SchedulePermissionResult.Deny("You can only manage your own visit types");

        var doctorId = await _uow.DoctorProfiles.GetIdByStaffIdAsync(targetStaffId, ct);
        return await CheckSelfManageLockAsync(doctorId, ct);
    }

    public async Task<SchedulePermissionResult> CanManageVisitTypesByDoctorIdAsync(
        Guid doctorProfileId, CancellationToken ct = default)
    {
        if (IsClinicOwner()) return SchedulePermissionResult.Allow();

        // Resolve current user's doctor profile
        var currentStaff = await GetCurrentStaffAsync(ct);
        if (currentStaff is null)
            return SchedulePermissionResult.Deny("You can only manage your own visit types");

        var currentDoctorId = await _uow.DoctorProfiles.GetIdByStaffIdAsync(currentStaff.Id, ct);
        if (currentDoctorId != doctorProfileId)
            return SchedulePermissionResult.Deny("You can only manage your own visit types");

        return await CheckSelfManageLockAsync(doctorProfileId, ct);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private bool IsClinicOwner() =>
        _currentUser.Roles.Contains(UserRoles.ClinicOwner);

    private async Task<Staff?> GetCurrentStaffAsync(CancellationToken ct) =>
        await _uow.Staff.GetByUserIdAsync(_currentUser.GetRequiredUserId(), ct);

    private async Task<SchedulePermissionResult> CheckSelfManageLockAsync(
        Guid doctorProfileId, CancellationToken ct)
    {
        if (doctorProfileId == Guid.Empty)
            return SchedulePermissionResult.Deny("Doctor profile not found");

        var doctor = await _uow.DoctorProfiles.GetByIdAsync(doctorProfileId, ct);
        if (doctor is not null && !doctor.CanSelfManageSchedule)
            return SchedulePermissionResult.Deny("Schedule management is locked by the clinic owner");

        return SchedulePermissionResult.Allow();
    }
}
