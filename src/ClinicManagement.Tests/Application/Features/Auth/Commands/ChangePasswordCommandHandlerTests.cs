using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Auth.Commands.ChangePassword;
using ClinicManagement.Domain.Entities;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Auth.Commands;

public class ChangePasswordCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUserManagementService> _userManagementServiceMock;
    private readonly ChangePasswordCommandHandler _handler;

    public ChangePasswordCommandHandlerTests()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _userManagementServiceMock = new Mock<IUserManagementService>();
        _handler = new ChangePasswordCommandHandler(
            _currentUserServiceMock.Object,
            _userManagementServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns((int?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authentication.USER_NOT_AUTHENTICATED, result.Message);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = 123;
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _userManagementServiceMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authentication.USER_NOT_FOUND, result.Message);
    }

    [Fact]
    public async Task Handle_WhenCurrentPasswordIncorrect_ShouldReturnFailure()
    {
        // Arrange
        var userId = 123;
        var user = new User { Id = userId, Email = "test@example.com" };
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "WrongPassword123!",
            NewPassword = "NewPassword123!"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _userManagementServiceMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userManagementServiceMock.Setup(x => x.CheckPasswordAsync(user, command.CurrentPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authentication.INVALID_PASSWORD, result.Message);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var userId = 123;
        var user = new User { Id = userId, Email = "test@example.com" };
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _userManagementServiceMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userManagementServiceMock.Setup(x => x.CheckPasswordAsync(user, command.CurrentPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
    }
}