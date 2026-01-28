using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Auth.Commands.ForgotPassword;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Auth.Commands;

public class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<IUserManagementService> _userManagementServiceMock;
    private readonly ForgotPasswordCommandHandler _handler;

    public ForgotPasswordCommandHandlerTests()
    {
        _userManagementServiceMock = new Mock<IUserManagementService>();
        _handler = new ForgotPasswordCommandHandler(_userManagementServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldReturnSuccessMessage()
    {
        // Arrange
        var command = new ForgotPasswordCommand { Email = "test@example.com" };
        var user = new User { Email = command.Email };
        
        _userManagementServiceMock.Setup(x => x.GetUserByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldReturnSameSuccessMessage()
    {
        // Arrange
        var command = new ForgotPasswordCommand { Email = "nonexistent@example.com" };
        
        _userManagementServiceMock.Setup(x => x.GetUserByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldCallUserManagementService()
    {
        // Arrange
        var command = new ForgotPasswordCommand { Email = "test@example.com" };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userManagementServiceMock.Verify(
            x => x.GetUserByEmailAsync(command.Email, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}