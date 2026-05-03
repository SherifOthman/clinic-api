using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Onboarding.Commands;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Moq;

namespace ClinicManagement.Application.Tests.Onboarding;

public class CompleteOnboardingHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IClinicRepository> _clinicsMock = new();
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly Mock<IBranchRepository> _branchesMock = new();
    private readonly Mock<IClinicMemberRepository> _membersMock = new();
    private readonly Mock<IPermissionRepository> _permissionsMock = new();
    private readonly Mock<IReferenceRepository> _referenceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly CompleteOnboardingHandler _handler;

    private readonly User _owner = TestHandlerHelpers.CreateTestUser("owner@test.com");
    private readonly Guid _planId = Guid.NewGuid();

    public CompleteOnboardingHandlerTests()
    {
        _uowMock.Setup(u => u.Clinics).Returns(_clinicsMock.Object);
        _uowMock.Setup(u => u.Users).Returns(_usersMock.Object);
        _uowMock.Setup(u => u.Branches).Returns(_branchesMock.Object);
        _uowMock.Setup(u => u.Members).Returns(_membersMock.Object);
        _uowMock.Setup(u => u.Permissions).Returns(_permissionsMock.Object);
        _uowMock.Setup(u => u.Reference).Returns(_referenceMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(_owner.Id);

        // Default happy-path setup
        _clinicsMock.Setup(x => x.ExistsByOwnerIdAsync(_owner.Id, default)).ReturnsAsync(false);
        _usersMock.Setup(x => x.GetByIdAsync(_owner.Id, default)).ReturnsAsync(_owner);
        _referenceMock.Setup(x => x.SubscriptionPlanExistsAsync(_planId, default)).ReturnsAsync(true);
        _clinicsMock.Setup(x => x.AddAsync(It.IsAny<Clinic>(), default)).Returns(Task.CompletedTask);
        _branchesMock.Setup(x => x.AddAsync(It.IsAny<ClinicBranch>(), default)).Returns(Task.CompletedTask);
        _membersMock.Setup(x => x.AddAsync(It.IsAny<ClinicMember>(), default)).Returns(Task.CompletedTask);
        _permissionsMock.Setup(x => x.SeedDefaultsAsync(It.IsAny<Guid>(), It.IsAny<Domain.Enums.ClinicMemberRole>(), default))
            .Returns(Task.CompletedTask);

        _handler = new CompleteOnboardingHandler(_uowMock.Object, _currentUserMock.Object);
    }

    private CompleteOnboarding MakeCommand() =>
        new("Test Clinic", _planId, "Main Branch", "123 Test St", 2, 3, null);

    [Fact]
    public async Task Handle_ShouldSucceed_WhenAllDataIsValid()
    {
        var result = await _handler.Handle(MakeCommand(), default);

        result.IsSuccess.Should().BeTrue();
        _clinicsMock.Verify(x => x.AddAsync(It.Is<Clinic>(c =>
            c.Name == "Test Clinic" && c.OnboardingCompleted), default), Times.Once);
        _branchesMock.Verify(x => x.AddAsync(It.Is<ClinicBranch>(b =>
            b.Name == "Main Branch" && b.IsMainBranch), default), Times.Once);
        _membersMock.Verify(x => x.AddAsync(It.Is<ClinicMember>(m =>
            m.UserId == _owner.Id && m.IsOwner), default), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserAlreadyOnboarded()
    {
        _clinicsMock.Setup(x => x.ExistsByOwnerIdAsync(_owner.Id, default)).ReturnsAsync(true);

        var result = await _handler.Handle(MakeCommand(), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.ALREADY_ONBOARDED);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserNotFound()
    {
        _usersMock.Setup(x => x.GetByIdAsync(_owner.Id, default)).ReturnsAsync((User?)null);

        var result = await _handler.Handle(MakeCommand(), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.USER_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenSubscriptionPlanNotFound()
    {
        var badPlanId = Guid.NewGuid();
        _referenceMock.Setup(x => x.SubscriptionPlanExistsAsync(badPlanId, default)).ReturnsAsync(false);

        var cmd = new CompleteOnboarding("Clinic", badPlanId, "Branch", "Addr", 1, 2, null);
        var result = await _handler.Handle(cmd, default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.PLAN_NOT_FOUND);
    }
}
