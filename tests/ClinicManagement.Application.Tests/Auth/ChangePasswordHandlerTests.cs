using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Auth.Commands.ChangePassword;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace ClinicManagement.Application.Tests.Handlers;

public class ChangePasswordHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<ILogger<ChangePasswordHandler>> _loggerMock = new();
    private readonly ChangePasswordHandler _handler;

    public ChangePasswordHandlerTests()
    {
        _context = TestHandlerHelpers.CreateInMemoryContext();
        _userManagerMock = TestHandlerHelpers.CreateMockUserManager();

        _handler = new ChangePasswordHandler(
            _context,
            _userManagerMock.Object,
            _currentUserMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _currentUserMock
            .Setup(x => x.GetRequiredUserId())
            .Returns(userId);

        var command = new ChangePasswordCommand("old", "new");

        var result = await _handler.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenCurrentPasswordIsIncorrect()
    {
        var user = TestHandlerHelpers.CreateTestUser();

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _currentUserMock
            .Setup(x => x.GetRequiredUserId())
            .Returns(user.Id);

        _userManagerMock
            .Setup(x => x.ChangePasswordAsync(user, "wrong", "new"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Incorrect password" }));

        var command = new ChangePasswordCommand("wrong", "new");

        var result = await _handler.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenCurrentPasswordIsCorrect()
    {
        var user = TestHandlerHelpers.CreateTestUser();

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _currentUserMock
            .Setup(x => x.GetRequiredUserId())
            .Returns(user.Id);

        _userManagerMock
            .Setup(x => x.ChangePasswordAsync(user, "old", "new"))
            .ReturnsAsync(IdentityResult.Success);

        var command = new ChangePasswordCommand("old", "new");

        var result = await _handler.Handle(command, default);

        result.IsSuccess.Should().BeTrue();

        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.LastPasswordChangeAt.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
