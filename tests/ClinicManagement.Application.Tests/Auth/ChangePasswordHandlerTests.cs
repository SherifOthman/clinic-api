using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
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
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly Mock<UserManager<User>> _userManagerMock = TestHandlerHelpers.CreateMockUserManager();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IAuditWriter> _auditWriterMock = new();
    private readonly ChangePasswordHandler _handler;

    public ChangePasswordHandlerTests()
    {
        _uowMock.Setup(u => u.Users).Returns(_usersMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        _handler = new ChangePasswordHandler(
            _uowMock.Object, _userManagerMock.Object, _currentUserMock.Object,
            _auditWriterMock.Object, NullLogger<ChangePasswordHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(userId);
        _usersMock.Setup(x => x.GetByIdAsync(userId, default)).ReturnsAsync((User?)null);

        var result = await _handler.Handle(new ChangePasswordCommand("old", "new"), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenCurrentPasswordIsWrong()
    {
        var user = TestHandlerHelpers.CreateTestUser();
        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(user.Id);
        _usersMock.Setup(x => x.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.ChangePasswordAsync(user, "wrong", "new"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Wrong password" }));

        var result = await _handler.Handle(new ChangePasswordCommand("wrong", "new"), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldSucceed_AndStampLastPasswordChange()
    {
        var user = TestHandlerHelpers.CreateTestUser();
        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(user.Id);
        _usersMock.Setup(x => x.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.ChangePasswordAsync(user, "old", "new"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _handler.Handle(new ChangePasswordCommand("old", "new"), default);

        result.IsSuccess.Should().BeTrue();
        user.LastPasswordChangeAt.Should().NotBeNull();
    }
}
