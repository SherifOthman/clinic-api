using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Auth.Commands.ResetPassword;
using ClinicManagement.Domain.Entities;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Auth.Commands;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IUserManagementService> _userManagementServiceMock;
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        _userManagementServiceMock = new Mock<IUserManagementService>();
        _handler = new ResetPasswordCommandHandler(_userManagementServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldReturnSuccess()
    {
        // Arrange
        var command = new ResetPasswordCommand { Email = "test@example.com", NewPassword = "newPassword123", Token = "reset-token" };
        var user = new User { Email = command.Email };
        
        _userManagementServiceMock.Setup(x => x.GetUserByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var command = new ResetPasswordCommand { Email = "nonexistent@example.com", NewPassword = "newPassword123", Token = "reset-token" };
        
        _userManagementServiceMock.Setup(x => x.GetUserByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ApplicationErrors.Authentication.INVALID_RESET_TOKEN, result.Message);
    }

    [Fact]
    public async Task Handle_ShouldCallUserManagementService()
    {
        // Arrange
        var command = new ResetPasswordCommand { Email = "test@example.com", NewPassword = "newPassword123", Token = "reset-token" };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userManagementServiceMock.Verify(
            x => x.GetUserByEmailAsync(command.Email, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}