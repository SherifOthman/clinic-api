using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Staff.Commands;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Moq;

namespace ClinicManagement.Application.Tests.Staff;

public class SaveWorkingDaysHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IDoctorInfoRepository> _doctorInfosMock = new();
    private readonly Mock<IDoctorScheduleRepository> _schedulesMock = new();
    private readonly Mock<IPermissionService> _permissionsMock = new();
    private readonly SaveWorkingDaysHandler _handler;

    private readonly Guid _staffId = Guid.NewGuid();
    private readonly Guid _branchId = Guid.NewGuid();
    private readonly Guid _doctorInfoId = Guid.NewGuid();

    public SaveWorkingDaysHandlerTests()
    {
        _uowMock.Setup(u => u.DoctorInfos).Returns(_doctorInfosMock.Object);
        _uowMock.Setup(u => u.DoctorSchedules).Returns(_schedulesMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        _handler = new SaveWorkingDaysHandler(_uowMock.Object, _permissionsMock.Object);
    }

    private void AllowPermission() =>
        _permissionsMock.Setup(x => x.CanManageScheduleAsync(_staffId, default))
            .ReturnsAsync(SchedulePermissionResult.Allow());

    private void DenyPermission(string reason) =>
        _permissionsMock.Setup(x => x.CanManageScheduleAsync(_staffId, default))
            .ReturnsAsync(SchedulePermissionResult.Deny(reason));

    [Fact]
    public async Task Handle_ShouldFail_WhenPermissionDenied()
    {
        DenyPermission("Schedule is locked");

        var result = await _handler.Handle(
            new SaveWorkingDaysCommand(_staffId, _branchId, []), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.FORBIDDEN);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenDoctorProfileNotFound()
    {
        AllowPermission();
        _doctorInfosMock.Setup(x => x.GetIdByMemberIdAsync(_staffId, default))
            .ReturnsAsync(Guid.Empty);

        var result = await _handler.Handle(
            new SaveWorkingDaysCommand(_staffId, _branchId, []), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.NOT_FOUND);
    }

    [Fact]
    public async Task Handle_ShouldSaveWorkingDays_WhenPermissionGranted()
    {
        AllowPermission();
        _doctorInfosMock.Setup(x => x.GetIdByMemberIdAsync(_staffId, default))
            .ReturnsAsync(_doctorInfoId);

        var schedule = new DoctorBranchSchedule { DoctorInfoId = _doctorInfoId, BranchId = _branchId };
        _schedulesMock.Setup(x => x.GetOrCreateScheduleAsync(_doctorInfoId, _branchId, default))
            .ReturnsAsync(schedule);
        _schedulesMock.Setup(x => x.GetWorkingDayEntitiesAsync(schedule.Id, default))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new SaveWorkingDaysCommand(_staffId, _branchId, [
            new(Day: 1, StartTime: "09:00", EndTime: "17:00", IsAvailable: true),
            new(Day: 2, StartTime: "09:00", EndTime: "17:00", IsAvailable: false),
        ]), default);

        result.IsSuccess.Should().BeTrue();
        _schedulesMock.Verify(x => x.AddWorkingDay(It.IsAny<WorkingDay>()), Times.Exactly(2));
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRemoveExistingDays_BeforeAddingNew()
    {
        AllowPermission();
        _doctorInfosMock.Setup(x => x.GetIdByMemberIdAsync(_staffId, default))
            .ReturnsAsync(_doctorInfoId);

        var schedule = new DoctorBranchSchedule { DoctorInfoId = _doctorInfoId, BranchId = _branchId };
        var existingDays = new List<WorkingDay>
        {
            new() { DoctorBranchScheduleId = schedule.Id, Day = DayOfWeek.Monday },
            new() { DoctorBranchScheduleId = schedule.Id, Day = DayOfWeek.Tuesday },
        };
        _schedulesMock.Setup(x => x.GetOrCreateScheduleAsync(_doctorInfoId, _branchId, default))
            .ReturnsAsync(schedule);
        _schedulesMock.Setup(x => x.GetWorkingDayEntitiesAsync(schedule.Id, default))
            .ReturnsAsync(existingDays);

        await _handler.Handle(new SaveWorkingDaysCommand(_staffId, _branchId, [
            new(Day: 5, StartTime: "10:00", EndTime: "14:00", IsAvailable: true),
        ]), default);

        _schedulesMock.Verify(x => x.RemoveWorkingDays(existingDays), Times.Once);
        _schedulesMock.Verify(x => x.AddWorkingDay(It.IsAny<WorkingDay>()), Times.Once);
    }
}
