using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Auth.Commands;

public class ConfirmEmailCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserManagementService> _userManagementServiceMock;
    private readonly Mock<IEmailConfirmationService> _emailConfirmationServiceMock;
    private readonly Mock<ILogger<ConfirmEmailCommandHandler>> _loggerMock;
    private readonly ConfirmEmailCommandHandler _handler;

    public ConfirmEmailCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userManagementServiceMock = new Mock<IUserManagementService>();
        _emailConfirmationServiceMock = new Mock<IEmailConfirmationService>();
        _loggerMock = new Mock<ILogger<ConfirmEmailCommandHandler>>();

        _handler = new ConfirmEmailCommandHandler(
            _unitOfWorkMock.Object,
            _userManagementServiceMock.Object,
            _emailConfirmationServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new ConfirmEmailCommand { Email = "test@example.com", Token = "token123" };
        _userManagementServiceMock.Setup(x => x.GetUserByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(ApplicationErrors.Authentication.USER_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyConfirmed_ShouldReturnSuccess()
    {
        // Arrange
        var command = new ConfirmEmailCommand { Email = "test@example.com", Token = "token123" };
        var user = new User { Email = command.Email };
        
        _userManagementServiceMock.Setup(x => x.GetUserByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _emailConfirmationServiceMock.Setup(x => x.IsEmailConfirmedAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenConfirmationSucceeds_ShouldReturnSuccess()
    {
        // Arrange
        var command = new ConfirmEmailCommand { Email = "test@example.com", Token = "token123" };
        var user = new User { Email = command.Email };
        var confirmationResult = Result.Ok();
        
        _userManagementServiceMock.Setup(x => x.GetUserByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _emailConfirmationServiceMock.Setup(x => x.IsEmailConfirmedAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _emailConfirmationServiceMock.Setup(x => x.ConfirmEmailAsync(user, command.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(confirmationResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenConfirmationFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new ConfirmEmailCommand { Email = "test@example.com", Token = "invalid-token" };
        var user = new User { Email = command.Email };
        var confirmationResult = Result.Fail("Invalid token");
        
        _userManagementServiceMock.Setup(x => x.GetUserByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _emailConfirmationServiceMock.Setup(x => x.IsEmailConfirmedAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _emailConfirmationServiceMock.Setup(x => x.ConfirmEmailAsync(user, command.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(confirmationResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid token");
    }

    [Fact]
    public async Task Handle_WhenConfirmationFailsWithoutMessage_ShouldReturnDefaultFailureMessage()
    {
        // Arrange
        var command = new ConfirmEmailCommand { Email = "test@example.com", Token = "invalid-token" };
        var user = new User { Email = command.Email };
        var confirmationResult = Result.Fail((string)null!);
        
        _userManagementServiceMock.Setup(x => x.GetUserByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _emailConfirmationServiceMock.Setup(x => x.IsEmailConfirmedAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _emailConfirmationServiceMock.Setup(x => x.ConfirmEmailAsync(user, command.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(confirmationResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Email confirmation failed.");
    }
}