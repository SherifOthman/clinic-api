using ClinicManagement.API.Controllers;
using ClinicManagement.Application.Features.Auth.Commands.Login;
using ClinicManagement.Application.Features.Auth.Commands.Logout;
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
        _controller = new AuthController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Register_WhenCommandIsValid_ShouldReturnOkResult()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            PhoneNumber = "+1234567890"
        };

        var expectedResult = Result.Ok();
        _mediatorMock.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Register(command, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Register_WhenCommandFails_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "invalid-email",
            Password = "weak",
            FirstName = "",
            LastName = "",
            Username = "",
            PhoneNumber = ""
        };

        var expectedResult = Result.Fail("Validation failed");
        _mediatorMock.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Register(command, CancellationToken.None);

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

        var expectedResult = Result.Ok();
        _mediatorMock.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Login(command, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkResult>();
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

        var expectedResult = Result.Fail("Invalid credentials");
        _mediatorMock.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Login(command, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Logout_ShouldReturnOkResult()
    {
        // Arrange
        var expectedResult = Result.Ok();
        _mediatorMock.Setup(x => x.Send(It.IsAny<LogoutCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Logout(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkResult>();
    }
}