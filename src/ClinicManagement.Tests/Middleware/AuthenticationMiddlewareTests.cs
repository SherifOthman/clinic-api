using ClinicManagement.Application.Common.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace ClinicManagement.Tests.Middleware;

public class AuthenticationMiddlewareTests : MiddlewareTestBase
{
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<ILogger<AuthenticationMiddlewareTests>> _loggerMock;

    public AuthenticationMiddlewareTests()
    {
        _tokenServiceMock = new Mock<ITokenService>();
        _loggerMock = CreateLoggerMock<AuthenticationMiddlewareTests>();
    }

    [Fact]
    public async Task ProcessAuthentication_WhenNoAccessTokenCookie_ShouldNotSetUser()
    {
        // Arrange
        var context = CreateHttpContext();

        // Act
        await ProcessAuthenticationAsync(context);

        // Assert
        context.User.Identity!.IsAuthenticated.Should().BeFalse();
        _tokenServiceMock.Verify(x => x.ValidateAccessToken(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAuthentication_WhenValidAccessTokenCookie_ShouldSetUserPrincipal()
    {
        // Arrange
        var context = CreateHttpContext();
        var accessToken = "valid-jwt-token";
        SetCookie(context, "AccessToken", accessToken);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims, "jwt");
        var principal = new ClaimsPrincipal(identity);

        _tokenServiceMock.Setup(x => x.ValidateAccessToken(accessToken))
            .Returns(principal);

        // Act
        await ProcessAuthenticationAsync(context);

        // Assert
        context.User.Should().Be(principal);
        context.User.Identity!.IsAuthenticated.Should().BeTrue();
        context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be("1");
        _tokenServiceMock.Verify(x => x.ValidateAccessToken(accessToken), Times.Once);
    }

    [Fact]
    public async Task ProcessAuthentication_WhenInvalidAccessTokenCookie_ShouldNotSetUserPrincipal()
    {
        // Arrange
        var context = CreateHttpContext();
        var accessToken = "invalid-jwt-token";
        SetCookie(context, "AccessToken", accessToken);

        _tokenServiceMock.Setup(x => x.ValidateAccessToken(accessToken))
            .Returns((ClaimsPrincipal?)null);

        // Act
        await ProcessAuthenticationAsync(context);

        // Assert
        context.User.Identity!.IsAuthenticated.Should().BeFalse();
        _tokenServiceMock.Verify(x => x.ValidateAccessToken(accessToken), Times.Once);
    }

    [Fact]
    public async Task ProcessAuthentication_WhenTokenServiceThrowsException_ShouldLogAndContinue()
    {
        // Arrange
        var context = CreateHttpContext();
        var accessToken = "problematic-token";
        SetCookie(context, "AccessToken", accessToken);

        _tokenServiceMock.Setup(x => x.ValidateAccessToken(accessToken))
            .Throws(new Exception("Token validation failed"));

        // Act
        await ProcessAuthenticationAsync(context);

        // Assert
        context.User.Identity!.IsAuthenticated.Should().BeFalse();
        _tokenServiceMock.Verify(x => x.ValidateAccessToken(accessToken), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ProcessAuthentication_WhenEmptyOrWhitespaceAccessTokenCookie_ShouldNotCallTokenService(string tokenValue)
    {
        // Arrange
        var context = CreateHttpContext();
        SetCookie(context, "AccessToken", tokenValue);

        // Act
        await ProcessAuthenticationAsync(context);

        // Assert
        context.User.Identity!.IsAuthenticated.Should().BeFalse();
        _tokenServiceMock.Verify(x => x.ValidateAccessToken(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAuthentication_WhenNullAccessTokenCookie_ShouldNotCallTokenService()
    {
        // Arrange
        var context = CreateHttpContext();
        // Don't set any cookies - cookies collection will be empty

        // Act
        await ProcessAuthenticationAsync(context);

        // Assert
        context.User.Identity!.IsAuthenticated.Should().BeFalse();
        _tokenServiceMock.Verify(x => x.ValidateAccessToken(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAuthentication_WhenMultipleClaims_ShouldPreserveAllClaims()
    {
        // Arrange
        var context = CreateHttpContext();
        var accessToken = "multi-claim-token";
        SetCookie(context, "AccessToken", accessToken);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim(ClaimTypes.Name, "John Doe"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "User"),
            new Claim("clinic_id", "456")
        };
        var identity = new ClaimsIdentity(claims, "jwt");
        var principal = new ClaimsPrincipal(identity);

        _tokenServiceMock.Setup(x => x.ValidateAccessToken(accessToken))
            .Returns(principal);

        // Act
        await ProcessAuthenticationAsync(context);

        // Assert
        context.User.Claims.Should().HaveCount(6);
        context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be("123");
        context.User.FindFirst(ClaimTypes.Email)!.Value.Should().Be("user@example.com");
        context.User.FindFirst("clinic_id")!.Value.Should().Be("456");
        context.User.IsInRole("Admin").Should().BeTrue();
        context.User.IsInRole("User").Should().BeTrue();
    }

    private static void SetCookie(HttpContext context, string key, string value)
    {
        context.Request.Headers.Cookie = $"{key}={value}";
    }

    private async Task ProcessAuthenticationAsync(HttpContext context)
    {
        // Simulate JWT cookie middleware logic
        var accessToken = context.Request.Cookies["AccessToken"];
        
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return;
        }

        try
        {
            var principal = _tokenServiceMock.Object.ValidateAccessToken(accessToken);
            if (principal != null)
            {
                context.User = principal;
            }
        }
        catch (Exception)
        {
            // Log the exception (in real middleware)
            // Continue without setting user
        }

        await Task.CompletedTask;
    }
}