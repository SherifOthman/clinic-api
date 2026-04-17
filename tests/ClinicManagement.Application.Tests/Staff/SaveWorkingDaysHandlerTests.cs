using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Staff.Commands;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Services;
using FluentAssertions;
using Moq;

namespace ClinicManagement.Application.Tests.Staff;

public class SaveWorkingDaysHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();

    private SaveWorkingDaysHandler CreateHandler(ICurrentUserService currentUser)
    {
        var permissions = new PermissionService(currentUser, _uow);
        return new SaveWorkingDaysHandler(_uow, permissions);
    }

    private SaveWorkingDaysHandler CreateHandlerAsOwner()
    {
        var svc = new Mock<ICurrentUserService>();
        svc.Setup(s => s.Roles).Returns([UserRoles.ClinicOwner]);
        return CreateHandler(svc.Object);
    }

    private SaveWorkingDaysHandler CreateHandlerAsDoctor(Guid userId)
    {
        var svc = new Mock<ICurrentUserService>();
        svc.Setup(s => s.Roles).Returns([UserRoles.Doctor]);
        svc.Setup(s => s.GetRequiredUserId()).Returns(userId);
        return CreateHandler(svc.Object);
    }

    private async Task<(Domain.Entities.Staff staff, Doctor dp, Guid branchId)> SeedDoctorAsync()
    {
        var spec  = TestHandlerHelpers.CreateTestSpecialization();
        await _uow.Specializations.AddAsync(spec);
        var staff = TestHandlerHelpers.CreateTestStaff();
        await _uow.Staff.AddAsync(staff);
        await _uow.SaveChangesAsync();

        var dp = TestHandlerHelpers.CreateTestDoctorProfile(staffId: staff.Id, specializationId: spec.Id);
        await _uow.DoctorProfiles.AddAsync(dp);

        var branch = new ClinicBranch
        {
            ClinicId = staff.ClinicId, Name = "Main Branch", AddressLine = "123 Test St",
            StateGeonameId = 2, CityGeonameId = 3,
            IsMainBranch = true, IsActive = true,
        };
        await _uow.Branches.AddAsync(branch);
        await _uow.SaveChangesAsync();
        return (staff, dp, branch.Id);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenDoctorProfileNotFound()
    {
        var handler = CreateHandlerAsOwner();
        var result = await handler.Handle(
            new SaveWorkingDaysCommand(Guid.NewGuid(), Guid.NewGuid(), []), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.NOT_FOUND);
    }

    [Fact]
    public async Task Handle_ShouldSaveWorkingDays_AsOwner()
    {
        var (staff, dp, branchId) = await SeedDoctorAsync();
        var handler = CreateHandlerAsOwner();

        var result = await handler.Handle(new SaveWorkingDaysCommand(staff.Id, branchId, [
            new(Day: 1, StartTime: "09:00", EndTime: "17:00", IsAvailable: true),
            new(Day: 2, StartTime: "09:00", EndTime: "17:00", IsAvailable: false),
        ]), default);

        result.IsSuccess.Should().BeTrue();

        var days = await _uow.WorkingDays.GetByDoctorProfileIdAsync(dp.Id);
        days.Should().HaveCount(2);
        days.Should().Contain(d => d.Day == (int)DayOfWeek.Monday && d.IsAvailable);
        days.Should().Contain(d => d.Day == (int)DayOfWeek.Tuesday && !d.IsAvailable);
    }

    [Fact]
    public async Task Handle_ShouldSaveWorkingDays_AsDoctor_WhenCanSelfManage()
    {
        var (staff, dp, branchId) = await SeedDoctorAsync();
        var handler = CreateHandlerAsDoctor(staff.UserId);

        var result = await handler.Handle(new SaveWorkingDaysCommand(staff.Id, branchId, [
            new(Day: 3, StartTime: "08:00", EndTime: "16:00", IsAvailable: true),
        ]), default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldFail_AsDoctor_WhenScheduleLocked()
    {
        var (staff, dp, branchId) = await SeedDoctorAsync();
        dp.CanSelfManageSchedule = false;
        await _uow.SaveChangesAsync();

        var handler = CreateHandlerAsDoctor(staff.UserId);
        var result = await handler.Handle(
            new SaveWorkingDaysCommand(staff.Id, branchId, []), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.FORBIDDEN);
    }

    [Fact]
    public async Task Handle_ShouldFail_AsDoctor_WhenEditingAnotherDoctorsSchedule()
    {
        var (staff, _, branchId) = await SeedDoctorAsync();
        // Different user ID — not the owner of this staff record
        var handler = CreateHandlerAsDoctor(Guid.NewGuid());

        var result = await handler.Handle(
            new SaveWorkingDaysCommand(staff.Id, branchId, []), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.FORBIDDEN);
    }

    [Fact]
    public async Task Handle_ShouldReplaceExistingDays_OnSecondSave()
    {
        var (staff, dp, branchId) = await SeedDoctorAsync();
        var handler = CreateHandlerAsOwner();

        await handler.Handle(new SaveWorkingDaysCommand(staff.Id, branchId, [
            new(1, "09:00", "17:00", true),
            new(2, "09:00", "17:00", true),
            new(3, "09:00", "17:00", true),
        ]), default);

        var result = await handler.Handle(new SaveWorkingDaysCommand(staff.Id, branchId, [
            new(5, "10:00", "14:00", true),
        ]), default);

        result.IsSuccess.Should().BeTrue();

        var days = await _uow.WorkingDays.GetByDoctorProfileIdAsync(dp.Id);
        days.Should().HaveCount(1);
        days.Single().Day.Should().Be((int)DayOfWeek.Friday);
    }
}
