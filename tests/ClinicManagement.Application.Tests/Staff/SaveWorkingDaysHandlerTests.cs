using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Staff.Commands;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Moq;

namespace ClinicManagement.Application.Tests.Staff;

public class SaveWorkingDaysHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();

    /// <summary>Creates a handler where the caller is a clinic owner (can edit any doctor).</summary>
    private SaveWorkingDaysHandler CreateHandlerAsOwner()
    {
        var svc = new Mock<ICurrentUserService>();
        svc.Setup(s => s.Roles).Returns([UserRoles.ClinicOwner]);
        return new SaveWorkingDaysHandler(_uow, svc.Object);
    }

    /// <summary>Creates a handler where the caller is the doctor themselves.</summary>
    private SaveWorkingDaysHandler CreateHandlerAsDoctor(Guid userId)
    {
        var svc = new Mock<ICurrentUserService>();
        svc.Setup(s => s.Roles).Returns([UserRoles.Doctor]);
        svc.Setup(s => s.GetRequiredUserId()).Returns(userId);
        return new SaveWorkingDaysHandler(_uow, svc.Object);
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
        // dp.CanSelfManageSchedule defaults to true
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
