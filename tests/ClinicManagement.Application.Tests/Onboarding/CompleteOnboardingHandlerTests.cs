using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Onboarding.Commands;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ClinicManagement.Application.Tests.Onboarding;

public class CompleteOnboardingHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<UserManager<User>> _userManagerMock = TestHandlerHelpers.CreateMockUserManager();
    private readonly CompleteOnboardingHandler _handler;
    private readonly User _owner;
    private readonly SubscriptionPlan _plan;
    private readonly Specialization _spec;

    public CompleteOnboardingHandlerTests()
    {
        _owner = TestHandlerHelpers.CreateTestUser("owner@test.com");
        _plan  = TestHandlerHelpers.CreateTestSubscriptionPlan();
        _spec  = TestHandlerHelpers.CreateTestSpecialization();

        _uow.UserEntities.Add(_owner);
        _uow.SubscriptionPlans.AddAsync(_plan).GetAwaiter().GetResult();
        _uow.Specializations.AddAsync(_spec).GetAwaiter().GetResult();
        _uow.SaveChangesAsync().GetAwaiter().GetResult();

        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(_owner.Id);
        _userManagerMock.Setup(x => x.GetRolesAsync(_owner))
            .ReturnsAsync([UserRoles.ClinicOwner]);

        _handler = new CompleteOnboardingHandler(_uow, _currentUserMock.Object, _userManagerMock.Object);
    }

    private CompleteOnboarding MakeCommand(bool provideMedical = true, Guid? specId = null) =>
        new("Test Clinic", _plan.Id, "Main Branch", "123 Test St", 1, 2, 3, provideMedical, specId ?? _spec.Id);

    [Fact]
    public async Task Handle_ShouldCreateClinicBranchStaffAndDoctorProfile_WhenDoctorOnboarding()
    {
        var result = await _handler.Handle(MakeCommand(), default);

        result.IsSuccess.Should().BeTrue();

        var clinic = await _uow.Clinics.GetByOwnerIdAsync(_owner.Id);
        clinic!.Name.Should().Be("Test Clinic");
        clinic.OnboardingCompleted.Should().BeTrue();

        var branch = await _uow.Branches.GetMainBranchIdAsync();
        branch.Should().NotBe(Guid.Empty);

        var staff = await _uow.Staff.GetByUserIdAsync(_owner.Id);
        staff.Should().NotBeNull();

        var dpId = await _uow.DoctorProfiles.GetIdByStaffIdAsync(staff!.Id);
        dpId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ShouldNotCreateStaffOrDoctorProfile_WhenAdminOnboarding()
    {
        var result = await _handler.Handle(MakeCommand(provideMedical: false), default);

        result.IsSuccess.Should().BeTrue();

        var staff = await _uow.Staff.GetByUserIdAsync(_owner.Id);
        staff.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserAlreadyOnboarded()
    {
        // Seed an existing clinic for this owner
        await _uow.Clinics.AddAsync(TestHandlerHelpers.CreateTestClinic(ownerUserId: _owner.Id, subscriptionPlanId: _plan.Id));
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(MakeCommand(), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.ALREADY_ONBOARDED);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserNotFound()
    {
        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(Guid.NewGuid());

        var result = await _handler.Handle(MakeCommand(), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.USER_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenSubscriptionPlanNotFound()
    {
        var cmd = new CompleteOnboarding("Clinic", Guid.NewGuid(), "Branch", "Addr", 1, 2, 3, false, null);

        var result = await _handler.Handle(cmd, default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.PLAN_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserLacksClinicOwnerRole()
    {
        _userManagerMock.Setup(x => x.GetRolesAsync(_owner)).ReturnsAsync(["Doctor"]);

        var result = await _handler.Handle(MakeCommand(), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.FORBIDDEN);
    }
}
