using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.Options;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace ClinicManagement.Tests.Infrastructure.Services;

public class TokenServiceTests
{
    private readonly Mock<IOptions<JwtOptions>> _jwtOptionsMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly Mock<ILogger<TokenService>> _loggerMock;
    private readonly TokenService _service;
    private readonly JwtOptions _jwtOptions;

    public TokenServiceTests()
    {
        _jwtOptionsMock = new Mock<IOptions<JwtOptions>>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _loggerMock = new Mock<ILogger<TokenService>>();

        _jwtOptions = new JwtOptions
        {
            Key = "ThisIsASecretKeyForJwtTokenGenerationThatIsLongEnough",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 15
        };

        _jwtOptionsMock.Setup(x => x.Value).Returns(_jwtOptions);
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

        _service = new TokenService(
            _jwtOptionsMock.Object,
            _refreshTokenServiceMock.Object,
            _dateTimeProviderMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void GenerateAccessToken_WhenValidUser_ShouldReturnValidJwtToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            ClinicId = 1
        };

        var roles = new List<string> { "ClinicOwner" };

        // Act
        var token = _service.GenerateAccessToken(user, roles, user.ClinicId);

        // Assert
        token.Should().NotBeNullOrEmpty();

        // Validate JWT structure
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "1");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "test@example.com");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "ClinicOwner");
        jwtToken.Claims.Should().Contain(c => c.Type == "ClinicId" && c.Value == "1");
        jwtToken.Issuer.Should().Be(_jwtOptions.Issuer);
        jwtToken.Audiences.Should().Contain(_jwtOptions.Audience);
    }

    [Fact]
    public void GenerateAccessToken_WhenUserWithoutClinic_ShouldIncludeEmptyClinicClaim()
    {
        // Arrange
        var user = new User
        {
            Id = 2,
            Email = "newuser@example.com",
            FirstName = "Jane",
            LastName = "Smith",
            ClinicId = null
        };

        var roles = new List<string> { "User" };

        // Act
        var token = _service.GenerateAccessToken(user, roles, user.ClinicId);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "2");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "newuser@example.com");
        jwtToken.Claims.Should().Contain(c => c.Type == "ClinicId" && c.Value == "");
    }

    [Fact]
    public void GenerateAccessToken_WhenUserWithMultipleRoles_ShouldIncludeAllRoles()
    {
        // Arrange
        var user = new User
        {
            Id = 3,
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "User",
            ClinicId = 1
        };

        var roles = new List<string> { "Admin", "ClinicOwner", "User" };

        // Act
        var token = _service.GenerateAccessToken(user, roles, user.ClinicId);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).Should().HaveCount(3);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "ClinicOwner");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_WhenValidUser_ShouldReturnRefreshToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com"
        };

        var expectedRefreshToken = new RefreshToken { Token = "refresh-token-123" };
        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshTokenAsync(user.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRefreshToken);

        // Act
        var result = await _service.GenerateRefreshTokenAsync(user, CancellationToken.None);

        // Assert
        result.Should().Be("refresh-token-123");
        _refreshTokenServiceMock.Verify(x => x.GenerateRefreshTokenAsync(user.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void GenerateAccessToken_ShouldSetCorrectExpiration()
    {
        // Arrange
        var user = new User
        {
            Id = 5,
            Email = "expiry@example.com",
            FirstName = "Expiry",
            LastName = "Test"
        };

        var roles = new List<string>();
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = _service.GenerateAccessToken(user, roles, null);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var expectedExpiry = beforeGeneration.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes);
        var actualExpiry = jwtToken.ValidTo;

        // Allow for a small time difference due to execution time
        actualExpiry.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void ValidateAccessToken_WhenValidToken_ShouldReturnClaimsPrincipal()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var roles = new List<string> { "User" };
        var token = _service.GenerateAccessToken(user, roles, null);

        // Act
        var result = _service.ValidateAccessToken(token);

        // Assert
        result.Should().NotBeNull();
        result!.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("1");
        result!.FindFirst(ClaimTypes.Email)?.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void ValidateAccessToken_WhenInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid-token";

        // Act
        var result = _service.ValidateAccessToken(invalidToken);

        // Assert
        result.Should().BeNull();
    }
}