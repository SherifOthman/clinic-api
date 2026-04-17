using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Single place for all "can user X do action Y?" logic.
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
        Guid targetMemberId, CancellationToken ct = default)
    {
        if (await IsOwnerAsync(ct)) return SchedulePermissionResult.Allow();

        var currentMemberId = await GetCurrentMemberIdAsync(ct);
        if (currentMemberId != targetMemberId)
            return SchedulePermissionResult.Deny("You can only manage your own schedule");

        var doctorInfoId = await _uow.DoctorInfos.GetIdByMemberIdAsync(targetMemberId, ct);
        return await CheckSelfManageLockAsync(doctorInfoId, ct);
    }

    public async Task<SchedulePermissionResult> CanManageVisitTypesAsync(
        Guid targetMemberId, CancellationToken ct = default)
    {
        if (await IsOwnerAsync(ct)) return SchedulePermissionResult.Allow();

        var currentMemberId = await GetCurrentMemberIdAsync(ct);
        if (currentMemberId != targetMemberId)
            return SchedulePermissionResult.Deny("You can only manage your own visit types");

        var doctorInfoId = await _uow.DoctorInfos.GetIdByMemberIdAsync(targetMemberId, ct);
        return await CheckSelfManageLockAsync(doctorInfoId, ct);
    }

    public async Task<SchedulePermissionResult> CanManageVisitTypesByDoctorIdAsync(
        Guid doctorInfoId, CancellationToken ct = default)
    {
        if (await IsOwnerAsync(ct)) return SchedulePermissionResult.Allow();

        var currentMemberId = await GetCurrentMemberIdAsync(ct);
        if (currentMemberId == Guid.Empty)
            return SchedulePermissionResult.Deny("You can only manage your own visit types");

        var currentDoctorInfoId = await _uow.DoctorInfos.GetIdByMemberIdAsync(currentMemberId, ct);
        if (currentDoctorInfoId != doctorInfoId)
            return SchedulePermissionResult.Deny("You can only manage your own visit types");

        return await CheckSelfManageLockAsync(doctorInfoId, ct);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<bool> IsOwnerAsync(CancellationToken ct)
    {
        if (_currentUser.Roles.Contains(UserRoles.ClinicOwner)) return true;
        var member = await _uow.Members.GetByUserIdAsync(_currentUser.GetRequiredUserId(), ct);
        return member?.Role == ClinicMemberRole.Owner;
    }

    private async Task<Guid> GetCurrentMemberIdAsync(CancellationToken ct)
    {
        var member = await _uow.Members.GetByUserIdAsync(_currentUser.GetRequiredUserId(), ct);
        return member?.Id ?? Guid.Empty;
    }

    private async Task<SchedulePermissionResult> CheckSelfManageLockAsync(Guid doctorInfoId, CancellationToken ct)
    {
        if (doctorInfoId == Guid.Empty)
            return SchedulePermissionResult.Allow();

        var doctorInfo = await _uow.DoctorInfos.GetByIdAsync(doctorInfoId, ct);
        if (doctorInfo is not null && !doctorInfo.CanSelfManageSchedule)
            return SchedulePermissionResult.Deny("Schedule management is locked by the clinic owner");

        return SchedulePermissionResult.Allow();
    }
}
