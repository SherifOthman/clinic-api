using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Auth.Commands.Login;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Auth.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _authenticationServiceMock;
    private readonly Mock<ICookieService> _cookieServiceMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _authenticationServiceMock = new Mock<IAuthenticationService>();
        _cookieServiceMock = new Mock<ICookieService>();
        
        _handler = new LoginCommandHandler(
            _authenticationServiceMock.Object,
            _cookieServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "ValidPassword123!"
        };

        _authenticationServiceMock.Setup(x => x.LoginAsync(command.Email, command.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ClinicManagement.Application.Common.Models.Result<LoginResult>.Ok(new LoginResult
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token"
            }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        _authenticationServiceMock.Verify(x => x.LoginAsync(command.Email, command.Password, It.IsAny<CancellationToken>()), Times.Once);
        _cookieServiceMock.Verify(x => x.SetAccessTokenCookie("access-token"), Times.Once);
        _cookieServiceMock.Verify(x => x.SetRefreshTokenCookie("refresh-token"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenInvalidCredentials_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        _authenticationServiceMock.Setup(x => x.LoginAsync(command.Email, command.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ClinicManagement.Application.Common.Models.Result<LoginResult>.Fail("Invalid credentials"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid credentials");
        _cookieServiceMock.Verify(x => x.SetAccessTokenCookie(It.IsAny<string>()), Times.Never);
        _cookieServiceMock.Verify(x => x.SetRefreshTokenCookie(It.IsAny<string>()), Times.Never);
    }
}