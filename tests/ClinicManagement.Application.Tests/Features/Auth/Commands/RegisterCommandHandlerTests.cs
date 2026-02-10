using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.Features.Auth.Commands.Register;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ClinicManagement.Application.Tests.Features.Auth.Commands;

public class RegisterCommandHandlerTests
{
    private readonly IUserRegistrationService _userRegistrationService;
    private readonly ILogger<RegisterCommandHandler> _logger;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _userRegistrationService = Substitute.For<IUserRegistrationService>();
        _logger = Substitute.For<ILogger<RegisterCommandHandler>>();
        _handler = new RegisterCommandHandler(_userRegistrationService, _logger);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            UserName = "johndoe",
            Email = "john.doe@example.com",
            Password = "Password123!",
            PhoneNumber = "+1234567890"
        };

        var userId = Guid.NewGuid();
        _userRegistrationService
            .RegisterUserAsync(Arg.Any<UserRegistrationRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Ok(userId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        
        await _userRegistrationService.Received(1).RegisterUserAsync(
            Arg.Is<UserRegistrationRequest>(r =>
                r.FirstName == command.FirstName &&
                r.LastName == command.LastName &&
                r.UserName == command.UserName &&
                r.Email == command.Email &&
                r.Password == command.Password &&
                r.PhoneNumber == command.PhoneNumber &&
                r.UserType == UserType.ClinicOwner &&
                r.EmailConfirmed == false &&
                r.OnboardingCompleted == false &&
                r.SendConfirmationEmail == true
            ),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_RegistrationFails_ReturnsFailureWithCode()
    {
        // Arrange
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123!"
        };

        _userRegistrationService
            .RegisterUserAsync(Arg.Any<UserRegistrationRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.FailSystem("REGISTRATION_FAILED", "User creation failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Code.Should().Be("REGISTRATION_FAILED");
    }

    [Fact]
    public async Task Handle_RegistrationFailsWithFieldErrors_ReturnsFailureWithErrors()
    {
        // Arrange
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "existing@example.com",
            Password = "Password123!"
        };

        var validationErrors = new Dictionary<string, List<string>>
        {
            ["email"] = new List<string> { "Email is already registered" }
        };

        _userRegistrationService
            .RegisterUserAsync(Arg.Any<UserRegistrationRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.FailValidation(validationErrors));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ValidationErrors.Should().NotBeNull();
        result.ValidationErrors.Should().ContainKey("email");
        result.ValidationErrors["email"].Should().Contain("Email is already registered");
    }

    [Fact]
    public async Task Handle_ValidRequest_SetsCorrectUserType()
    {
        // Arrange
        var command = new RegisterCommand
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Password = "Password123!"
        };

        var userId = Guid.NewGuid();
        _userRegistrationService
            .RegisterUserAsync(Arg.Any<UserRegistrationRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Ok(userId));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _userRegistrationService.Received(1).RegisterUserAsync(
            Arg.Is<UserRegistrationRequest>(r => r.UserType == UserType.ClinicOwner),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_ValidRequest_SetsEmailConfirmedToFalse()
    {
        // Arrange
        var command = new RegisterCommand
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Password = "Password123!"
        };

        var userId = Guid.NewGuid();
        _userRegistrationService
            .RegisterUserAsync(Arg.Any<UserRegistrationRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Ok(userId));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _userRegistrationService.Received(1).RegisterUserAsync(
            Arg.Is<UserRegistrationRequest>(r => r.EmailConfirmed == false),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_ValidRequest_SetsSendConfirmationEmailToTrue()
    {
        // Arrange
        var command = new RegisterCommand
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Password = "Password123!"
        };

        var userId = Guid.NewGuid();
        _userRegistrationService
            .RegisterUserAsync(Arg.Any<UserRegistrationRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Ok(userId));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _userRegistrationService.Received(1).RegisterUserAsync(
            Arg.Is<UserRegistrationRequest>(r => r.SendConfirmationEmail == true),
            Arg.Any<CancellationToken>()
        );
    }
}
