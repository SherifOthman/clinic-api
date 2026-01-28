using ClinicManagement.API.Controllers;
using ClinicManagement.Application.Features.Auth.Commands.Login;
using ClinicManagement.Application.Features.Auth.Commands.Register;
using ClinicManagement.Application.Common.Models;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.API.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AuthController();
        
        // Use reflection to set the mediator since it's protected
        var mediatorField = typeof(BaseApiController).GetField("_mediator", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        mediatorField?.SetValue(_controller, _mediatorMock.Object);
    }

    [Fact]
    public async Task Register_WhenCommandIsValid_ShouldReturnOkResult()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe"
        };

        var expectedResult = Result.Ok();
        _mediatorMock.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Register(command);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Register_WhenCommandFails_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "invalid-email",
            Password = "weak",
            ConfirmPassword = "different",
            FirstName = "",
            LastName = "",
            Username = ""
        };

        var expectedResult = Result.Fail("Validation failed");
        _mediatorMock.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Register(command);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ShouldReturnOkResult()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var loginResponse = new LoginResponse
        {
            AccessToken = "access-token",
            User = new Application.DTOs.UserDto
            {
                Id = 1,
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe"
            }
        };

        var expectedResult = Result<LoginResponse>.Ok(loginResponse);
        _mediatorMock.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Login(command);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Login_WhenCredentialsAreInvalid_ShouldReturnUnauthorized()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "wrong-password"
        };

        var expectedResult = Result<LoginResponse>.Fail("Invalid credentials");
        _mediatorMock.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Login(command);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Logout_ShouldReturnOkResult()
    {
        // Act
        var result = await _controller.Logout();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(new { message = "Logged out successfully" });
    }
}