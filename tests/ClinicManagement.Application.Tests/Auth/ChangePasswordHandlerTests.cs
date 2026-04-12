using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Auth.Commands.ChangePassword;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ClinicManagement.Application.Tests.Auth;

public class ChangePasswordHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly Mock<UserManager<User>> _userManagerMock = TestHandlerHelpers.CreateMockUserManager();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly ChangePasswordHandler _handler;

    public ChangePasswordHandlerTests()
    {
        _handler = new ChangePasswordHandler(
            _uow, _userManagerMock.Object, _currentUserMock.Object,
            NullLogger<ChangePasswordHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserNotFound()
    {
        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(Guid.NewGuid());

        var result = await _handler.Handle(new ChangePasswordCommand("old", "new"), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenCurrentPasswordIsWrong()
    {
        var user = TestHandlerHelpers.CreateTestUser();
        _uow.UserEntities.Add(user);
        await _uow.SaveChangesAsync();
        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(user.Id);
        _userManagerMock.Setup(x => x.ChangePasswordAsync(It.IsAny<User>(), "wrong", "new"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Wrong password" }));

        var result = await _handler.Handle(new ChangePasswordCommand("wrong", "new"), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldSucceed_AndStampLastPasswordChange()
    {
        var user = TestHandlerHelpers.CreateTestUser();
        _uow.UserEntities.Add(user);
        await _uow.SaveChangesAsync();
        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(user.Id);
        _userManagerMock.Setup(x => x.ChangePasswordAsync(It.IsAny<User>(), "old", "new"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _handler.Handle(new ChangePasswordCommand("old", "new"), default);

        result.IsSuccess.Should().BeTrue();

        var updated = await _uow.Users.GetByIdAsync(user.Id);
        updated!.LastPasswordChangeAt.Should().NotBeNull();
    }
}
