using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Single place for all "can user X do action Y?" logic.
/// Supports both old (Staff/Doctor) and new (ClinicMember/DoctorInfo) models during migration.
/// To add a new role: update IsOwnerAsync() only.
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
        Guid targetId, CancellationToken ct = default)
    {
        if (await IsOwnerAsync(ct)) return SchedulePermissionResult.Allow();

        var currentId = await GetCurrentMemberOrStaffIdAsync(ct);
        if (currentId != targetId)
            return SchedulePermissionResult.Deny("You can only manage your own schedule");

        var doctorId = await ResolveDoctorIdAsync(targetId, ct);
        return await CheckSelfManageLockAsync(doctorId, ct);
    }

    public async Task<SchedulePermissionResult> CanManageVisitTypesAsync(
        Guid targetId, CancellationToken ct = default)
    {
        if (await IsOwnerAsync(ct)) return SchedulePermissionResult.Allow();

        var currentId = await GetCurrentMemberOrStaffIdAsync(ct);
        if (currentId != targetId)
            return SchedulePermissionResult.Deny("You can only manage your own visit types");

        var doctorId = await ResolveDoctorIdAsync(targetId, ct);
        return await CheckSelfManageLockAsync(doctorId, ct);
    }

    public async Task<SchedulePermissionResult> CanManageVisitTypesByDoctorIdAsync(
        Guid doctorId, CancellationToken ct = default)
    {
        if (await IsOwnerAsync(ct)) return SchedulePermissionResult.Allow();

        var currentMemberOrStaffId = await GetCurrentMemberOrStaffIdAsync(ct);
        if (currentMemberOrStaffId == Guid.Empty)
            return SchedulePermissionResult.Deny("You can only manage your own visit types");

        var currentDoctorId = await ResolveDoctorIdAsync(currentMemberOrStaffId, ct);
        if (currentDoctorId != doctorId)
            return SchedulePermissionResult.Deny("You can only manage your own visit types");

        return await CheckSelfManageLockAsync(doctorId, ct);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<bool> IsOwnerAsync(CancellationToken ct)
    {
        // Fast path: Identity role (always available)
        if (_currentUser.Roles.Contains(UserRoles.ClinicOwner)) return true;

        // New model: ClinicMember.Role
        var userId = _currentUser.GetRequiredUserId();
        var member = await _uow.Members.GetByUserIdAsync(userId, ct);
        return member?.Role == ClinicMemberRole.Owner;
    }

    /// <summary>
    /// Returns the current user's ClinicMember.Id (new model) or Staff.Id (old model).
    /// Both are used as the "member ID" in permission checks.
    /// </summary>
    private async Task<Guid> GetCurrentMemberOrStaffIdAsync(CancellationToken ct)
    {
        var userId = _currentUser.GetRequiredUserId();

        // Try new model first
        var member = await _uow.Members.GetByUserIdAsync(userId, ct);
        if (member is not null) return member.Id;

        // Fall back to old Staff model
        var staff = await _uow.Staff.GetByUserIdAsync(userId, ct);
        return staff?.Id ?? Guid.Empty;
    }

    /// <summary>
    /// Resolves a DoctorInfo.Id (new) or Doctor.Id (old) from a member/staff ID.
    /// </summary>
    private async Task<Guid> ResolveDoctorIdAsync(Guid memberOrStaffId, CancellationToken ct)
    {
        // Try new model
        var doctorInfoId = await _uow.DoctorInfos.GetIdByMemberIdAsync(memberOrStaffId, ct);
        if (doctorInfoId != Guid.Empty) return doctorInfoId;

        // Fall back to old Doctor model
        return await _uow.DoctorProfiles.GetIdByStaffIdAsync(memberOrStaffId, ct);
    }

    /// <summary>
    /// Checks CanSelfManageSchedule on DoctorInfo (new) or Doctor (old).
    /// </summary>
    private async Task<SchedulePermissionResult> CheckSelfManageLockAsync(Guid doctorId, CancellationToken ct)
    {
        if (doctorId == Guid.Empty)
            return SchedulePermissionResult.Allow(); // not a doctor — no lock applies

        // Try new DoctorInfo model
        var doctorInfo = await _uow.DoctorInfos.GetByIdAsync(doctorId, ct);
        if (doctorInfo is not null)
            return doctorInfo.CanSelfManageSchedule
                ? SchedulePermissionResult.Allow()
                : SchedulePermissionResult.Deny("Schedule management is locked by the clinic owner");

        // Fall back to old Doctor model
        var doctor = await _uow.DoctorProfiles.GetByIdAsync(doctorId, ct);
        if (doctor is not null)
            return doctor.CanSelfManageSchedule
                ? SchedulePermissionResult.Allow()
                : SchedulePermissionResult.Deny("Schedule management is locked by the clinic owner");

        return SchedulePermissionResult.Allow();
    }
}
