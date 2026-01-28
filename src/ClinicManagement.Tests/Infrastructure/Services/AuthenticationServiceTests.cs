using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Infrastructure.Services;

public class AuthenticationServiceTests
{
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IUserManagementService> _userManagementServiceMock;
    private readonly Mock<IEmailConfirmationService> _emailConfirmationServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<ILogger<AuthenticationService>> _loggerMock;
    private readonly AuthenticationService _service;

    public AuthenticationServiceTests()
    {
        _tokenServiceMock = new Mock<ITokenService>();
        _userManagementServiceMock = new Mock<IUserManagementService>();
        _emailConfirmationServiceMock = new Mock<IEmailConfirmationService>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _loggerMock = new Mock<ILogger<AuthenticationService>>();

        _service = new AuthenticationService(
            _tokenServiceMock.Object,
            _userManagementServiceMock.Object,
            _emailConfirmationServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task LoginAsync_WhenValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var email = "test@example.com";
        var password = "ValidPassword123!";

        var user = new User
        {
            Id = 1,
            Email = email,
            ClinicId = 1
        };

        _userManagementServiceMock.Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userManagementServiceMock.Setup(x => x.CheckPasswordAsync(user, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _emailConfirmationServiceMock.Setup(x => x.IsEmailConfirmedAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _userManagementServiceMock.Setup(x => x.GetUserRolesAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "User" });

        _tokenServiceMock.Setup(x => x.GenerateAccessToken(user, It.IsAny<IEnumerable<string>>(), user.ClinicId))
            .Returns("access-token");

        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshTokenAsync(user.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshToken { Token = "refresh-token" });

        // Act
        var result = await _service.LoginAsync(email, password, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("access-token");
        result.Value!.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var password = "password";

        _userManagementServiceMock.Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.LoginAsync(email, password, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("Invalid username or password");
    }

    [Fact]
    public async Task LoginAsync_WhenEmailNotConfirmed_ShouldReturnFailure()
    {
        // Arrange
        var email = "test@example.com";
        var password = "ValidPassword123!";

        var user = new User
        {
            Id = 1,
            Email = email
        };

        _userManagementServiceMock.Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userManagementServiceMock.Setup(x => x.CheckPasswordAsync(user, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _emailConfirmationServiceMock.Setup(x => x.IsEmailConfirmedAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.LoginAsync(email, password, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("Please verify your email address before signing in. Check your inbox for the verification link.");
    }

    [Fact]
    public async Task LoginAsync_WhenInvalidPassword_ShouldReturnFailure()
    {
        // Arrange
        var email = "test@example.com";
        var password = "WrongPassword";

        var user = new User
        {
            Id = 1,
            Email = email
        };

        _userManagementServiceMock.Setup(x => x.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userManagementServiceMock.Setup(x => x.CheckPasswordAsync(user, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.LoginAsync(email, password, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("Invalid username or password");
    }

    [Fact]
    public async Task LogoutAsync_WhenCalled_ShouldRevokeRefreshToken()
    {
        // Arrange
        var refreshToken = "refresh-token";

        // Act
        var result = await _service.LogoutAsync(refreshToken, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        _refreshTokenServiceMock.Verify(x => x.RevokeRefreshTokenAsync(refreshToken, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_WhenNoRefreshToken_ShouldStillReturnSuccess()
    {
        // Arrange
        string? refreshToken = null;

        // Act
        var result = await _service.LogoutAsync(refreshToken, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        _refreshTokenServiceMock.Verify(x => x.RevokeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}