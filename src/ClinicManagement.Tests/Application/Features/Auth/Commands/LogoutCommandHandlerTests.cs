using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Auth.Commands.Logout;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Auth.Commands;

public class LogoutCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _authenticationServiceMock;
    private readonly Mock<ICookieService> _cookieServiceMock;
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _authenticationServiceMock = new Mock<IAuthenticationService>();
        _cookieServiceMock = new Mock<ICookieService>();
        
        _handler = new LogoutCommandHandler(
            _authenticationServiceMock.Object,
            _cookieServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldLogoutAndClearCookies()
    {
        // Arrange
        var command = new LogoutCommand();
        var refreshToken = "refresh-token";

        _cookieServiceMock.Setup(x => x.GetRefreshTokenFromCookie())
            .Returns(refreshToken);

        _authenticationServiceMock.Setup(x => x.LogoutAsync(refreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ClinicManagement.Application.Common.Models.Result<bool>.Ok(true));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        _authenticationServiceMock.Verify(x => x.LogoutAsync(refreshToken, It.IsAny<CancellationToken>()), Times.Once);
        _cookieServiceMock.Verify(x => x.ClearAuthCookies(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoRefreshToken_ShouldStillClearCookies()
    {
        // Arrange
        var command = new LogoutCommand();

        _cookieServiceMock.Setup(x => x.GetRefreshTokenFromCookie())
            .Returns((string?)null);

        _authenticationServiceMock.Setup(x => x.LogoutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ClinicManagement.Application.Common.Models.Result<bool>.Ok(true));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        _cookieServiceMock.Verify(x => x.ClearAuthCookies(), Times.Once);
    }
}