using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.Features.Auth.Commands.Register;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Auth.Commands;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserManagementService> _userManagementServiceMock;
    private readonly Mock<IEmailConfirmationService> _emailConfirmationServiceMock;
    private readonly Mock<ILogger<RegisterCommandHandler>> _loggerMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _userManagementServiceMock = new Mock<IUserManagementService>();
        _emailConfirmationServiceMock = new Mock<IEmailConfirmationService>();
        _loggerMock = new Mock<ILogger<RegisterCommandHandler>>();
        
        var phoneNumberValidationServiceMock = new Mock<IPhoneNumberValidationService>();
        phoneNumberValidationServiceMock.Setup(x => x.GetE164Format(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((phone, region) => phone); // Return phone as-is for tests
        
        _handler = new RegisterCommandHandler(
            _userManagementServiceMock.Object,
            _emailConfirmationServiceMock.Object,
            phoneNumberValidationServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidData_ShouldReturnSuccess()
    {
        // Arrange
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            Email = "john@example.com",
            Password = "ValidPassword123!",
            PhoneNumber = "+1234567890"
        };

        _userManagementServiceMock.Setup(x => x.GetUserByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userManagementServiceMock.Setup(x => x.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userManagementServiceMock.Setup(x => x.CreateUserAsync(It.IsAny<User>(), command.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ClinicManagement.Application.Common.Models.Result.Ok());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        _userManagementServiceMock.Verify(x => x.CreateUserAsync(It.IsAny<User>(), command.Password, It.IsAny<CancellationToken>()), Times.Once);
        _emailConfirmationServiceMock.Verify(x => x.SendConfirmationEmailAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailExists_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            Email = "existing@example.com",
            Password = "ValidPassword123!",
            PhoneNumber = "+1234567890"
        };

        var existingUser = new User { Email = command.Email };
        _userManagementServiceMock.Setup(x => x.GetUserByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors!.Should().Contain(e => e.Field == "email" && e.Message == MessageCodes.Validation.EMAIL_ALREADY_REGISTERED);
        _userManagementServiceMock.Verify(x => x.CreateUserAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUsernameExists_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Username = "existinguser",
            Email = "john@example.com",
            Password = "ValidPassword123!",
            PhoneNumber = "+1234567890"
        };

        _userManagementServiceMock.Setup(x => x.GetUserByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var existingUser = new User { UserName = command.Username };
        _userManagementServiceMock.Setup(x => x.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors!.Should().Contain(e => e.Field == "username" && e.Message == MessageCodes.Validation.USERNAME_ALREADY_TAKEN);
        _userManagementServiceMock.Verify(x => x.CreateUserAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}