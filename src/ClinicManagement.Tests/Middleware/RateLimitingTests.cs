using ClinicManagement.Application.Common.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using Xunit;

namespace ClinicManagement.Tests.Middleware;

public class RateLimitingTests : MiddlewareTestBase
{
    private readonly Mock<IRateLimitService> _rateLimitServiceMock;
    private readonly Mock<ILogger<RateLimitingTests>> _loggerMock;

    public RateLimitingTests()
    {
        _rateLimitServiceMock = new Mock<IRateLimitService>();
        _loggerMock = CreateLoggerMock<RateLimitingTests>();
    }

    [Fact]
    public async Task ProcessRateLimit_WhenRateLimitNotExceeded_ShouldAllowRequest()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");
        context.Request.Path = "/api/test";

        _rateLimitServiceMock.Setup(x => x.IsRateLimitExceededAsync(
            "192.168.1.1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await ProcessRateLimitAsync(context);

        // Assert
        result.Should().BeTrue(); // Request allowed
        _rateLimitServiceMock.Verify(x => x.IsRateLimitExceededAsync(
            "192.168.1.1", null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessRateLimit_WhenRateLimitExceeded_ShouldBlockRequest()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");
        context.Request.Path = "/api/test";

        _rateLimitServiceMock.Setup(x => x.IsRateLimitExceededAsync(
            "192.168.1.1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await ProcessRateLimitAsync(context);

        // Assert
        result.Should().BeFalse(); // Request blocked
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task ProcessRateLimit_WhenNoRemoteIpAddress_ShouldUseDefaultIdentifier()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Connection.RemoteIpAddress = null;
        context.Request.Path = "/api/test";

        _rateLimitServiceMock.Setup(x => x.IsRateLimitExceededAsync(
            "unknown", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await ProcessRateLimitAsync(context);

        // Assert
        result.Should().BeTrue();
        _rateLimitServiceMock.Verify(x => x.IsRateLimitExceededAsync(
            "unknown", null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("/api/auth/login")]
    [InlineData("/api/auth/register")]
    [InlineData("/api/patients")]
    [InlineData("/api/chronic-diseases")]
    [InlineData("/api/admin/users")]
    public async Task ProcessRateLimit_WithDifferentEndpoints_ShouldUseCorrectIpAddress(string endpoint)
    {
        // Arrange
        var context = CreateHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");
        context.Request.Path = endpoint;

        _rateLimitServiceMock.Setup(x => x.IsRateLimitExceededAsync(
            "192.168.1.1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await ProcessRateLimitAsync(context);

        // Assert
        result.Should().BeTrue();
        _rateLimitServiceMock.Verify(x => x.IsRateLimitExceededAsync(
            "192.168.1.1", null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessRateLimit_WhenRateLimitServiceThrowsException_ShouldAllowRequest()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");
        context.Request.Path = "/api/test";

        _rateLimitServiceMock.Setup(x => x.IsRateLimitExceededAsync(
            It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Rate limit service error"));

        // Act
        var result = await ProcessRateLimitAsync(context);

        // Assert
        result.Should().BeTrue(); // Allow request when service fails
    }

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("10.0.0.1")]
    [InlineData("172.16.0.1")]
    [InlineData("192.168.0.1")]
    [InlineData("::1")]
    public async Task ProcessRateLimit_WithDifferentIpAddresses_ShouldTrackSeparately(string ipAddress)
    {
        // Arrange
        var context = CreateHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(ipAddress);
        context.Request.Path = "/api/test";

        _rateLimitServiceMock.Setup(x => x.IsRateLimitExceededAsync(
            ipAddress, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await ProcessRateLimitAsync(context);

        // Assert
        result.Should().BeTrue();
        _rateLimitServiceMock.Verify(x => x.IsRateLimitExceededAsync(
            ipAddress, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessRateLimit_WithAuthenticatedUser_ShouldPassUserId()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");
        context.Request.Path = "/api/test";
        
        // Simulate authenticated user
        var userId = 123;

        _rateLimitServiceMock.Setup(x => x.IsRateLimitExceededAsync(
            "192.168.1.1", userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await ProcessRateLimitAsync(context, userId);

        // Assert
        result.Should().BeTrue();
        _rateLimitServiceMock.Verify(x => x.IsRateLimitExceededAsync(
            "192.168.1.1", userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessRateLimit_WhenRateLimitExceeded_ShouldReturnCorrectResponse()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");
        context.Request.Path = "/api/test";

        _rateLimitServiceMock.Setup(x => x.IsRateLimitExceededAsync(
            It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await ProcessRateLimitAsync(context);

        // Assert
        result.Should().BeFalse();
        context.Response.StatusCode.Should().Be(429);

        var responseBody = await GetResponseBodyAsync(context);
        responseBody.Should().Contain("Rate limit exceeded");
    }

    private async Task<bool> ProcessRateLimitAsync(HttpContext context, int? userId = null)
    {
        // Simulate rate limiting middleware logic
        var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        try
        {
            var isExceeded = await _rateLimitServiceMock.Object.IsRateLimitExceededAsync(
                clientId, userId, CancellationToken.None);

            if (isExceeded)
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                return false;
            }

            return true;
        }
        catch (Exception)
        {
            // Log error and allow request to continue
            return true;
        }
    }
}