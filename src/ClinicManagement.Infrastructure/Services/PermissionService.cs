using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Centralizes "can the current user do X?" checks for schedule/visit-type management.
/// Uses the permissions system — checks ClinicMemberPermission rows rather than roles.
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
        Guid targetMemberId, CancellationToken ct = default)
    {
        var currentMember = await GetCurrentMemberAsync(ct);
        if (currentMember is null) return SchedulePermissionResult.Deny("Not a clinic member");

        // Owner always allowed
        if (currentMember.IsOwner) return SchedulePermissionResult.Allow();

        // Must have ManageSchedule permission
        var permissions = await _uow.Permissions.GetByMemberIdAsync(currentMember.Id, ct);
        if (!permissions.Contains(Permission.ManageSchedule))
            return SchedulePermissionResult.Deny("You do not have permission to manage schedules");

        // Non-owners can only manage their own schedule
        if (currentMember.Id != targetMemberId)
            return SchedulePermissionResult.Deny("You can only manage your own schedule");

        return await CheckSelfManageLockAsync(currentMember.Id, ct);
    }

    public async Task<SchedulePermissionResult> CanManageVisitTypesAsync(
        Guid targetMemberId, CancellationToken ct = default)
    {
        var currentMember = await GetCurrentMemberAsync(ct);
        if (currentMember is null) return SchedulePermissionResult.Deny("Not a clinic member");

        if (currentMember.IsOwner) return SchedulePermissionResult.Allow();

        var permissions = await _uow.Permissions.GetByMemberIdAsync(currentMember.Id, ct);
        if (!permissions.Contains(Permission.ManageVisitTypes))
            return SchedulePermissionResult.Deny("You do not have permission to manage visit types");

        if (currentMember.Id != targetMemberId)
            return SchedulePermissionResult.Deny("You can only manage your own visit types");

        var doctorInfoId = await _uow.DoctorInfos.GetIdByMemberIdAsync(targetMemberId, ct);
        return await CheckSelfManageLockAsync(doctorInfoId, ct);
    }

    public async Task<SchedulePermissionResult> CanManageVisitTypesByDoctorIdAsync(
        Guid doctorInfoId, CancellationToken ct = default)
    {
        var currentMember = await GetCurrentMemberAsync(ct);
        if (currentMember is null) return SchedulePermissionResult.Deny("Not a clinic member");

        if (currentMember.IsOwner) return SchedulePermissionResult.Allow();

        var permissions = await _uow.Permissions.GetByMemberIdAsync(currentMember.Id, ct);
        if (!permissions.Contains(Permission.ManageVisitTypes))
            return SchedulePermissionResult.Deny("You do not have permission to manage visit types");

        var currentDoctorInfoId = await _uow.DoctorInfos.GetIdByMemberIdAsync(currentMember.Id, ct);
        if (currentDoctorInfoId != doctorInfoId)
            return SchedulePermissionResult.Deny("You can only manage your own visit types");

        return await CheckSelfManageLockAsync(doctorInfoId, ct);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<ClinicMember?> GetCurrentMemberAsync(CancellationToken ct)
        => await _uow.Members.GetByUserIdAsync(_currentUser.GetRequiredUserId(), ct);

    private async Task<SchedulePermissionResult> CheckSelfManageLockAsync(Guid doctorInfoId, CancellationToken ct)
    {
        if (doctorInfoId == Guid.Empty) return SchedulePermissionResult.Allow();
        var doctorInfo = await _uow.DoctorInfos.GetByIdAsync(doctorInfoId, ct);
        return doctorInfo is not null && !doctorInfo.CanSelfManageSchedule
            ? SchedulePermissionResult.Deny("Schedule management is locked by the clinic owner")
            : SchedulePermissionResult.Allow();
    }
}
