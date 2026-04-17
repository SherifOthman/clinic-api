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

    private async Task<(ClinicMember member, DoctorInfo doctorInfo, Guid branchId)> SeedDoctorAsync()
    {
        var spec = TestHandlerHelpers.CreateTestSpecialization();
        await _uow.Specializations.AddAsync(spec);

        var (_, member) = TestHandlerHelpers.CreateTestMember();
        await _uow.Members.AddAsync(member);
        await _uow.SaveChangesAsync();

        var doctorInfo = TestHandlerHelpers.CreateTestDoctorInfo(member.Id, spec.Id);
        await _uow.DoctorInfos.AddAsync(doctorInfo);

        var branch = new ClinicBranch
        {
            ClinicId = member.ClinicId, Name = "Main Branch", AddressLine = "123 Test St",
            StateGeonameId = 2, CityGeonameId = 3, IsMainBranch = true, IsActive = true,
        };
        await _uow.Branches.AddAsync(branch);
        await _uow.SaveChangesAsync();
        return (member, doctorInfo, branch.Id);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenDoctorProfileNotFound()
    {
        var handler = CreateHandlerAsOwner();
        var result  = await handler.Handle(new SaveWorkingDaysCommand(Guid.NewGuid(), Guid.NewGuid(), []), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.NOT_FOUND);
    }

    [Fact]
    public async Task Handle_ShouldSaveWorkingDays_AsOwner()
    {
        var (member, _, branchId) = await SeedDoctorAsync();
        var handler = CreateHandlerAsOwner();

        var result = await handler.Handle(new SaveWorkingDaysCommand(member.Id, branchId, [
            new(Day: 1, StartTime: "09:00", EndTime: "17:00", IsAvailable: true),
            new(Day: 2, StartTime: "09:00", EndTime: "17:00", IsAvailable: false),
        ]), default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldSaveWorkingDays_AsDoctor_WhenCanSelfManage()
    {
        var (member, _, branchId) = await SeedDoctorAsync();
        var handler = CreateHandlerAsDoctor(member.UserId!.Value);

        var result = await handler.Handle(new SaveWorkingDaysCommand(member.Id, branchId, [
            new(Day: 3, StartTime: "08:00", EndTime: "16:00", IsAvailable: true),
        ]), default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldFail_AsDoctor_WhenScheduleLocked()
    {
        var (member, doctorInfo, branchId) = await SeedDoctorAsync();
        doctorInfo.CanSelfManageSchedule = false;
        await _uow.SaveChangesAsync();

        var handler = CreateHandlerAsDoctor(member.UserId!.Value);
        var result  = await handler.Handle(new SaveWorkingDaysCommand(member.Id, branchId, []), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.FORBIDDEN);
    }

    [Fact]
    public async Task Handle_ShouldFail_AsDoctor_WhenEditingAnotherDoctorsSchedule()
    {
        var (member, _, branchId) = await SeedDoctorAsync();
        var handler = CreateHandlerAsDoctor(Guid.NewGuid());

        var result = await handler.Handle(new SaveWorkingDaysCommand(member.Id, branchId, []), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.FORBIDDEN);
    }

    [Fact]
    public async Task Handle_ShouldReplaceExistingDays_OnSecondSave()
    {
        var (member, _, branchId) = await SeedDoctorAsync();
        var handler = CreateHandlerAsOwner();

        await handler.Handle(new SaveWorkingDaysCommand(member.Id, branchId, [
            new(1, "09:00", "17:00", true),
            new(2, "09:00", "17:00", true),
        ]), default);

        var result = await handler.Handle(new SaveWorkingDaysCommand(member.Id, branchId, [
            new(5, "10:00", "14:00", true),
        ]), default);

        result.IsSuccess.Should().BeTrue();
    }
}
