using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Auth.Commands.ResendEmailVerification;
using ClinicManagement.Domain.Entities;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Auth.Commands;

public class ResendEmailVerificationCommandHandlerTests
{
    private readonly Mock<IUserManagementService> _userManagementServiceMock;
    private readonly Mock<IEmailConfirmationService> _emailConfirmationServiceMock;
    private readonly ResendEmailVerificationCommandHandler _handler;

    public ResendEmailVerificationCommandHandlerTests()
    {
        _userManagementServiceMock = new Mock<IUserManagementService>();
        _emailConfirmationServiceMock = new Mock<IEmailConfirmationService>();
        _handler = new ResendEmailVerificationCommandHandler(
            _userManagementServiceMock.Object,
            _emailConfirmationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new ResendEmailVerificationCommand { Email = "test@example.com" };
        _userManagementServiceMock.Setup(x => x.GetUserByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ApplicationErrors.Authentication.UserWithEmailNotFound("test@example.com"), result.Code);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyConfirmed_ShouldReturnFailure()
    {
        // Arrange
        var command = new ResendEmailVerificationCommand { Email = "test@example.com" };
        var user = new User { Email = command.Email };
        
        _userManagementServiceMock.Setup(x => x.GetUserByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _emailConfirmationServiceMock.Setup(x => x.IsEmailConfirmedAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ApplicationErrors.Authentication.EMAIL_ALREADY_CONFIRMED, result.Code);
    }

    [Fact]
    public async Task Handle_WhenSendEmailFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new ResendEmailVerificationCommand { Email = "test@example.com" };
        var user = new User { Email = command.Email };
        var emailResult = Result.Fail("SMTP error");
        
        _userManagementServiceMock.Setup(x => x.GetUserByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _emailConfirmationServiceMock.Setup(x => x.IsEmailConfirmedAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _emailConfirmationServiceMock.Setup(x => x.SendConfirmationEmailAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emailResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Failed to send confirmation email: SMTP error", result.Code);
    }

    [Fact]
    public async Task Handle_WhenSuccessful_ShouldReturnSuccess()
    {
        // Arrange
        var command = new ResendEmailVerificationCommand { Email = "test@example.com" };
        var user = new User { Email = command.Email };
        var emailResult = Result.Ok();
        
        _userManagementServiceMock.Setup(x => x.GetUserByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _emailConfirmationServiceMock.Setup(x => x.IsEmailConfirmedAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _emailConfirmationServiceMock.Setup(x => x.SendConfirmationEmailAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emailResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
    }
}