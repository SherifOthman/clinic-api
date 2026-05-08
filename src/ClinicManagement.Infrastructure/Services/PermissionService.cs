using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Centralizes "can the current user do X?" checks for schedule/visit-type management.
/// MemberId and role are read directly from JWT claims — no DB call needed.
/// Only the DoctorInfo.CanSelfManageSchedule flag requires a DB lookup.
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly ICurrentUserService   _currentUser;
    private readonly IPermissionRepository _permissions;
    private readonly IDoctorInfoRepository _doctorInfos;

    public PermissionService(
        ICurrentUserService currentUser,
        IPermissionRepository permissions,
        IDoctorInfoRepository doctorInfos)
    {
        _currentUser = currentUser;
        _permissions = permissions;
        _doctorInfos = doctorInfos;
    }

    public async Task<SchedulePermissionResult> CanManageScheduleAsync(
        Guid targetMemberId, CancellationToken ct = default)
    {
        if (IsOwner()) return SchedulePermissionResult.Allow();

        var memberId = _currentUser.MemberId;
        if (memberId is null) return SchedulePermissionResult.Deny("Not a clinic member");

        var perms = await _permissions.GetByMemberIdAsync(memberId.Value, ct);
        if (!perms.Contains(Permission.ManageSchedule))
            return SchedulePermissionResult.Deny("You do not have permission to manage schedules");

        if (memberId.Value != targetMemberId)
            return SchedulePermissionResult.Deny("You can only manage your own schedule");

        var doctorInfoId = await _doctorInfos.GetIdByMemberIdAsync(memberId.Value, ct);
        return await CheckSelfManageLockAsync(doctorInfoId, ct);
    }

    public async Task<SchedulePermissionResult> CanManageVisitTypesAsync(
        Guid targetMemberId, CancellationToken ct = default)
    {
        if (IsOwner()) return SchedulePermissionResult.Allow();

        var memberId = _currentUser.MemberId;
        if (memberId is null) return SchedulePermissionResult.Deny("Not a clinic member");

        var perms = await _permissions.GetByMemberIdAsync(memberId.Value, ct);
        if (!perms.Contains(Permission.ManageVisitTypes))
            return SchedulePermissionResult.Deny("You do not have permission to manage visit types");

        if (memberId.Value != targetMemberId)
            return SchedulePermissionResult.Deny("You can only manage your own visit types");

        var doctorInfoId = await _doctorInfos.GetIdByMemberIdAsync(memberId.Value, ct);
        return await CheckSelfManageLockAsync(doctorInfoId, ct);
    }

    public async Task<SchedulePermissionResult> CanManageVisitTypesByDoctorIdAsync(
        Guid doctorInfoId, CancellationToken ct = default)
    {
        if (IsOwner()) return SchedulePermissionResult.Allow();

        var memberId = _currentUser.MemberId;
        if (memberId is null) return SchedulePermissionResult.Deny("Not a clinic member");

        var perms = await _permissions.GetByMemberIdAsync(memberId.Value, ct);
        if (!perms.Contains(Permission.ManageVisitTypes))
            return SchedulePermissionResult.Deny("You do not have permission to manage visit types");

        var currentDoctorInfoId = await _doctorInfos.GetIdByMemberIdAsync(memberId.Value, ct);
        if (currentDoctorInfoId != doctorInfoId)
            return SchedulePermissionResult.Deny("You can only manage your own visit types");

        return await CheckSelfManageLockAsync(doctorInfoId, ct);
    }

    private bool IsOwner() => _currentUser.Roles.Contains(UserRoles.ClinicOwner);

    private async Task<SchedulePermissionResult> CheckSelfManageLockAsync(Guid doctorInfoId, CancellationToken ct)
    {
        if (doctorInfoId == Guid.Empty) return SchedulePermissionResult.Allow();
        var doctorInfo = await _doctorInfos.GetByIdAsync(doctorInfoId, ct);
        return doctorInfo is not null && !doctorInfo.CanSelfManageSchedule
            ? SchedulePermissionResult.Deny("Schedule management is locked by the clinic owner")
            : SchedulePermissionResult.Allow();
    }
}
