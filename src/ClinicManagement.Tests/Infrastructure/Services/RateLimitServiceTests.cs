using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Infrastructure.Services;

public class RateLimitServiceTests : IDisposable
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<RateLimitService>> _loggerMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly Mock<IRateLimitRepository> _rateLimitRepositoryMock;
    private readonly RateLimitService _rateLimitService;
    private readonly DateTime _currentTime = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    public RateLimitServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<RateLimitService>>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _rateLimitRepositoryMock = new Mock<IRateLimitRepository>();

        _unitOfWorkMock.Setup(x => x.RateLimitEntries).Returns(_rateLimitRepositoryMock.Object);
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(_currentTime);

        _rateLimitService = new RateLimitService(
            _unitOfWorkMock.Object,
            _memoryCache,
            _loggerMock.Object,
            _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task IsRateLimitExceededAsync_WhenCacheHasCountBelowLimit_ShouldReturnFalseAndIncrementCache()
    {
        // Arrange
        var ipAddress = "192.168.1.1";
        var cacheKey = "rate_limit_IP_192.168.1.1";
        var currentCount = 50;
        
        _memoryCache.Set(cacheKey, currentCount, TimeSpan.FromMinutes(1));

        // Act
        var result = await _rateLimitService.IsRateLimitExceededAsync(ipAddress);

        // Assert
        result.Should().BeFalse();
        
        // Verify cache was incremented
        var updatedCount = _memoryCache.Get<int>(cacheKey);
        updatedCount.Should().Be(51);
    }

    [Fact]
    public async Task IsRateLimitExceededAsync_WhenCacheHasCountAtLimit_ShouldReturnTrue()
    {
        // Arrange
        var ipAddress = "192.168.1.1";
        var cacheKey = "rate_limit_IP_192.168.1.1";
        var currentCount = 100; // At IP limit
        
        _memoryCache.Set(cacheKey, currentCount, TimeSpan.FromMinutes(1));

        // Act
        var result = await _rateLimitService.IsRateLimitExceededAsync(ipAddress);

        // Assert
        result.Should().BeTrue();
        
        // Verify cache was not incremented since limit exceeded
        var cacheCount = _memoryCache.Get<int>(cacheKey);
        cacheCount.Should().Be(100);
    }

    [Fact]
    public async Task IsRateLimitExceededAsync_WhenUserAuthenticated_ShouldUseHigherLimit()
    {
        // Arrange
        var ipAddress = "192.168.1.1";
        var userId = 123;
        var cacheKey = "rate_limit_USER_123";
        var currentCount = 150; // Below user limit (200) but above IP limit (100)
        
        _memoryCache.Set(cacheKey, currentCount, TimeSpan.FromMinutes(1));

        // Act
        var result = await _rateLimitService.IsRateLimitExceededAsync(ipAddress, userId);

        // Assert
        result.Should().BeFalse();
        
        // Verify cache was incremented
        var updatedCount = _memoryCache.Get<int>(cacheKey);
        updatedCount.Should().Be(151);
    }

    [Fact]
    public async Task IsRateLimitExceededAsync_WhenUserAtLimit_ShouldReturnTrue()
    {
        // Arrange
        var ipAddress = "192.168.1.1";
        var userId = 123;
        var cacheKey = "rate_limit_USER_123";
        var currentCount = 200; // At user limit
        
        _memoryCache.Set(cacheKey, currentCount, TimeSpan.FromMinutes(1));

        // Act
        var result = await _rateLimitService.IsRateLimitExceededAsync(ipAddress, userId);

        // Assert
        result.Should().BeTrue();
        
        // Verify cache was not incremented since limit exceeded
        var cacheCount = _memoryCache.Get<int>(cacheKey);
        cacheCount.Should().Be(200);
    }

    [Fact]
    public async Task IsRateLimitExceededAsync_WhenCacheMissAndNoDbEntry_ShouldCreateNewEntryAndReturnFalse()
    {
        // Arrange
        var ipAddress = "192.168.1.1";
        
        _rateLimitRepositoryMock.Setup(x => x.GetActiveEntryAsync(
            ipAddress, "IP", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RateLimitEntry?)null);

        // Act
        var result = await _rateLimitService.IsRateLimitExceededAsync(ipAddress);

        // Assert
        result.Should().BeFalse();
        _rateLimitRepositoryMock.Verify(x => x.AddAsync(
            It.Is<RateLimitEntry>(e => 
                e.Identifier == ipAddress && 
                e.Type == "IP" && 
                e.RequestCount == 1), 
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IsRateLimitExceededAsync_WhenDbEntryExistsAndBelowLimit_ShouldIncrementAndReturnFalse()
    {
        // Arrange
        var ipAddress = "192.168.1.1";
        var existingEntry = new RateLimitEntry
        {
            Identifier = ipAddress,
            Type = "IP",
            RequestCount = 50,
            WindowStart = _currentTime,
            ExpiresAt = _currentTime.AddMinutes(1)
        };

        _rateLimitRepositoryMock.Setup(x => x.GetActiveEntryAsync(
            ipAddress, "IP", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntry);

        // Act
        var result = await _rateLimitService.IsRateLimitExceededAsync(ipAddress);

        // Assert
        result.Should().BeFalse();
        existingEntry.RequestCount.Should().Be(51);
        _rateLimitRepositoryMock.Verify(x => x.UpdateAsync(existingEntry, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IsRateLimitExceededAsync_WhenDbEntryAtLimit_ShouldReturnTrue()
    {
        // Arrange
        var ipAddress = "192.168.1.1";
        var existingEntry = new RateLimitEntry
        {
            Identifier = ipAddress,
            Type = "IP",
            RequestCount = 100, // At limit
            WindowStart = _currentTime,
            ExpiresAt = _currentTime.AddMinutes(1)
        };

        _rateLimitRepositoryMock.Setup(x => x.GetActiveEntryAsync(
            ipAddress, "IP", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntry);

        // Act
        var result = await _rateLimitService.IsRateLimitExceededAsync(ipAddress);

        // Assert
        result.Should().BeTrue();
        existingEntry.RequestCount.Should().Be(100); // Should not increment
        _rateLimitRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<RateLimitEntry>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task IsRateLimitExceededAsync_WhenDatabaseThrowsException_ShouldLogErrorAndAllowRequest()
    {
        // Arrange
        var ipAddress = "192.168.1.1";
        
        _rateLimitRepositoryMock.Setup(x => x.GetActiveEntryAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _rateLimitService.IsRateLimitExceededAsync(ipAddress);

        // Assert
        result.Should().BeFalse(); // Should allow request on error
        
        // Verify error was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error checking rate limit")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task IsRateLimitExceededAsync_WhenCacheExceeded_ShouldLogWarning()
    {
        // Arrange
        var ipAddress = "192.168.1.1";
        var cacheKey = "rate_limit_IP_192.168.1.1";
        var currentCount = 100; // At limit
        
        _memoryCache.Set(cacheKey, currentCount, TimeSpan.FromMinutes(1));

        // Act
        await _rateLimitService.IsRateLimitExceededAsync(ipAddress);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Rate limit exceeded in cache")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task IsRateLimitExceededAsync_WhenDbLimitExceeded_ShouldLogWarning()
    {
        // Arrange
        var ipAddress = "192.168.1.1";
        var existingEntry = new RateLimitEntry
        {
            Identifier = ipAddress,
            Type = "IP",
            RequestCount = 100, // At limit
            WindowStart = _currentTime,
            ExpiresAt = _currentTime.AddMinutes(1)
        };

        _rateLimitRepositoryMock.Setup(x => x.GetActiveEntryAsync(
            ipAddress, "IP", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntry);

        // Act
        await _rateLimitService.IsRateLimitExceededAsync(ipAddress);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Rate limit exceeded in database")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    public void Dispose()
    {
        _memoryCache?.Dispose();
    }
}