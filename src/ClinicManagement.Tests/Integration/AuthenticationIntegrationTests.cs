using ClinicManagement.Application.Common.Behaviors;
using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.Features.Auth.Commands.Login;
using ClinicManagement.Application.Features.Auth.Commands.Register;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Services;
using FluentAssertions;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Integration;

public class AuthenticationIntegrationTests : ApiTestBase
{
    private readonly IMediator _mediator;
    private readonly IAuthenticationService _authenticationService;
    private readonly Mock<IEmailConfirmationService> _emailConfirmationServiceMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;

    public AuthenticationIntegrationTests()
    {
        // Setup mocks
        _emailConfirmationServiceMock = new Mock<IEmailConfirmationService>();
        _tokenServiceMock = new Mock<ITokenService>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();

        // Create services
        var services = new ServiceCollection();
        services.AddSingleton(_context);
        services.AddSingleton(_userManager);
        services.AddSingleton(_emailConfirmationServiceMock.Object);
        services.AddSingleton(_tokenServiceMock.Object);
        services.AddSingleton(_refreshTokenServiceMock.Object);
        services.AddSingleton<IUserManagementService, UserManagementService>();
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IPhoneNumberValidationService, PhoneNumberValidationService>();
        services.AddLogging();

        // Add Mapster and register mappings
        services.AddMapster();
        ClinicManagement.Application.Common.Mappings.MappingConfig.RegisterMappings();

        // Add MediatR with validation behavior
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(typeof(LoginCommand).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // Add validators
        services.AddValidatorsFromAssembly(typeof(RegisterCommandValidator).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        _mediator = serviceProvider.GetRequiredService<IMediator>();
        _authenticationService = serviceProvider.GetRequiredService<IAuthenticationService>();
    }

    [Fact]
    public async Task RegisterUser_WhenValidData_ShouldCreateUser()
    {
        // Arrange
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            Email = "john@example.com",
            Password = "ValidPassword123!",
            PhoneNumber = "+12025551234" // Valid US phone number
        };

        _emailConfirmationServiceMock.Setup(x => x.SendConfirmationEmailAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Success.Should().BeTrue();

        // Verify user was created
        var user = await _userManager.FindByEmailAsync(command.Email);
        user.Should().NotBeNull();
        user!.FirstName.Should().Be(command.FirstName);
        user.LastName.Should().Be(command.LastName);
    }

    [Fact]
    public async Task RegisterUser_WhenEmailExists_ShouldReturnError()
    {
        // Arrange
        await CreateTestUserAsync("existing@example.com", "Password123!", "User");

        var command = new RegisterCommand
        {
            FirstName = "Jane",
            LastName = "Doe",
            Username = "janedoe",
            Email = "existing@example.com", // This email already exists
            Password = "ValidPassword123!",
            PhoneNumber = "+12025551235" // Valid US phone number
        };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors!.Should().Contain(e => e.Field == "email");
    }

    [Fact]
    public async Task LoginUser_WhenValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var user = await CreateTestUserAsync("test@example.com", "TestPassword123!", "User");
        
        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IEnumerable<string>>(), It.IsAny<int?>()))
            .Returns("access-token");

        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshTokenAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshToken { Token = "refresh-token" });

        _emailConfirmationServiceMock.Setup(x => x.IsEmailConfirmedAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _authenticationService.LoginAsync("test@example.com", "TestPassword123!");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AccessToken.Should().Be("access-token");
        result.Value.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task LoginUser_WhenInvalidPassword_ShouldReturnError()
    {
        // Arrange
        await CreateTestUserAsync("test@example.com", "TestPassword123!", "User");

        // Act
        var result = await _authenticationService.LoginAsync("test@example.com", "WrongPassword");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid username or password");
    }

    [Fact]
    public async Task LoginUser_WhenEmailNotConfirmed_ShouldReturnError()
    {
        // Arrange
        var user = await CreateTestUserAsync("test@example.com", "TestPassword123!", "User");
        
        _emailConfirmationServiceMock.Setup(x => x.IsEmailConfirmedAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _authenticationService.LoginAsync("test@example.com", "TestPassword123!");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("verify your email");
    }

    [Fact]
    public async Task LoginUser_WhenUserNotFound_ShouldReturnError()
    {
        // Act
        var result = await _authenticationService.LoginAsync("nonexistent@example.com", "Password123!");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid username or password");
    }
}