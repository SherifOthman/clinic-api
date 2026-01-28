using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Options;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Infrastructure.Services;

public class RefreshTokenServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly Mock<ILogger<RefreshTokenService>> _loggerMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly RefreshTokenService _refreshTokenService;
    private readonly JwtOptions _jwtOptions;

    public RefreshTokenServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _loggerMock = new Mock<ILogger<RefreshTokenService>>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        _jwtOptions = new JwtOptions
        {
            RefreshTokenExpirationDays = 7
        };

        _unitOfWorkMock.Setup(x => x.RefreshTokens).Returns(_refreshTokenRepositoryMock.Object);

        _refreshTokenService = new RefreshTokenService(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _dateTimeProviderMock.Object,
            Options.Create(_jwtOptions),
            _loggerMock.Object);
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_WhenValidUserId_ShouldCreateAndReturnToken()
    {
        // Arrange
        var userId = 123;
        var ipAddress = "192.168.1.1";
        var now = DateTime.UtcNow;
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(now);
        _currentUserServiceMock.Setup(x => x.IpAddress).Returns(ipAddress);

        // Act
        var result = await _refreshTokenService.GenerateRefreshTokenAsync(userId, ipAddress);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(now, result.CreatedAt);
        Assert.Equal(now.AddDays(_jwtOptions.RefreshTokenExpirationDays), result.ExpiryTime);
        Assert.Equal(ipAddress, result.CreatedByIp);

        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetActiveRefreshTokenAsync_WhenValidToken_ShouldReturnToken()
    {
        // Arrange
        var token = "valid-refresh-token";
        var userId = 123;
        var now = DateTime.UtcNow;
        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = userId,
            CreatedAt = now,
            ExpiryTime = now.AddDays(7),
            IsRevoked = false
        };

        _refreshTokenRepositoryMock.Setup(x => x.GetActiveTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        // Act
        var result = await _refreshTokenService.GetActiveRefreshTokenAsync(token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(token, result.Token);
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async Task GetActiveRefreshTokenAsync_WhenTokenNotFound_ShouldReturnNull()
    {
        // Arrange
        var token = "non-existent-token";
        _refreshTokenRepositoryMock.Setup(x => x.GetActiveTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await _refreshTokenService.GetActiveRefreshTokenAsync(token);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_WhenValidToken_ShouldRevokeToken()
    {
        // Arrange
        var token = "valid-refresh-token";
        var userId = 123;
        var ipAddress = "192.168.1.1";
        var now = DateTime.UtcNow;
        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = userId,
            CreatedAt = now,
            ExpiryTime = now.AddDays(7),
            IsRevoked = false
        };

        _refreshTokenRepositoryMock.Setup(x => x.GetActiveTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(now);
        _currentUserServiceMock.Setup(x => x.IpAddress).Returns(ipAddress);

        // Act
        await _refreshTokenService.RevokeRefreshTokenAsync(token, ipAddress);

        // Assert
        Assert.True(refreshToken.IsRevoked);
        Assert.Equal(now, refreshToken.RevokedAt);
        Assert.Equal(ipAddress, refreshToken.RevokedByIp);

        _refreshTokenRepositoryMock.Verify(x => x.UpdateAsync(refreshToken, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_WhenTokenNotFound_ShouldNotThrow()
    {
        // Arrange
        var token = "non-existent-token";
        _refreshTokenRepositoryMock.Setup(x => x.GetActiveTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act & Assert
        await _refreshTokenService.RevokeRefreshTokenAsync(token);

        _refreshTokenRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeAllUserRefreshTokensAsync_WhenValidUserId_ShouldRevokeAllTokens()
    {
        // Arrange
        var userId = 123;
        var ipAddress = "192.168.1.1";
        var now = DateTime.UtcNow;
        var userTokens = new List<RefreshToken>
        {
            new() { Token = "token1", UserId = userId, CreatedAt = now, ExpiryTime = now.AddDays(7), IsRevoked = false },
            new() { Token = "token2", UserId = userId, CreatedAt = now, ExpiryTime = now.AddDays(7), IsRevoked = false }
        };

        _refreshTokenRepositoryMock.Setup(x => x.GetActiveTokensByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userTokens);
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(now);
        _currentUserServiceMock.Setup(x => x.IpAddress).Returns(ipAddress);

        // Act
        await _refreshTokenService.RevokeAllUserRefreshTokensAsync(userId, ipAddress);

        // Assert
        Assert.All(userTokens, token => 
        {
            Assert.True(token.IsRevoked);
            Assert.Equal(now, token.RevokedAt);
            Assert.Equal(ipAddress, token.RevokedByIp);
        });

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeAllUserRefreshTokensAsync_WhenNoActiveTokens_ShouldNotSaveChanges()
    {
        // Arrange
        var userId = 123;
        var userTokens = new List<RefreshToken>();

        _refreshTokenRepositoryMock.Setup(x => x.GetActiveTokensByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userTokens);

        // Act
        await _refreshTokenService.RevokeAllUserRefreshTokensAsync(userId);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CleanupExpiredTokensAsync_WhenExpiredTokensExist_ShouldReturnCleanedCount()
    {
        // Arrange
        var expectedCleanedCount = 5;
        _refreshTokenRepositoryMock.Setup(x => x.DeleteExpiredTokensAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCleanedCount);

        // Act
        var result = await _refreshTokenService.CleanupExpiredTokensAsync();

        // Assert
        Assert.Equal(expectedCleanedCount, result);
    }

    [Fact]
    public async Task CleanupExpiredTokensAsync_WhenNoExpiredTokens_ShouldReturnZero()
    {
        // Arrange
        _refreshTokenRepositoryMock.Setup(x => x.DeleteExpiredTokensAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _refreshTokenService.CleanupExpiredTokensAsync();

        // Assert
        Assert.Equal(0, result);
    }
}