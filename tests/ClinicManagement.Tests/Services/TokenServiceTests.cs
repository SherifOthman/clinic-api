using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Common.Options;
using ClinicManagement.API.Entities;
using ClinicManagement.API.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ClinicManagement.Tests.Services;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;
    private readonly JwtOptions _jwtOptions;
    private readonly DateTimeProvider _dateTimeProvider;

    public TokenServiceTests()
    {
        _jwtOptions = new JwtOptions
        {
            Key = "ThisIsAVerySecureKeyForTestingPurposesOnly12345678901234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        _dateTimeProvider = new DateTimeProvider();
        var logger = Mock.Of<ILogger<TokenService>>();
        _tokenService = new TokenService(Options.Create(_jwtOptions), _dateTimeProvider, logger);
    }

    [Fact]
    public void GenerateAccessToken_ShouldGenerateValidToken()
    {
        // Arrange
        var user = CreateTestUser();
        var roles = new[] { "Doctor" };
        var clinicId = Guid.NewGuid();

        // Act
        var token = _tokenService.GenerateAccessToken(user, roles, clinicId);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        jwtToken.Issuer.Should().Be(_jwtOptions.Issuer);
        jwtToken.Audiences.Should().Contain(_jwtOptions.Audience);
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeUserClaims()
    {
        // Arrange
        var user = CreateTestUser();
        var roles = new[] { "Doctor", "Admin" };
        var clinicId = Guid.NewGuid();

        // Act
        var token = _tokenService.GenerateAccessToken(user, roles, clinicId);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == $"{user.FirstName} {user.LastName}");
        jwtToken.Claims.Should().Contain(c => c.Type == "ClinicId" && c.Value == clinicId.ToString());
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeRoles()
    {
        // Arrange
        var user = CreateTestUser();
        var roles = new[] { "Doctor", "Admin" };

        // Act
        var token = _tokenService.GenerateAccessToken(user, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).Should().HaveCount(2);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Doctor");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void GenerateAccessToken_ShouldSetExpirationTime()
    {
        // Arrange
        var user = CreateTestUser();
        var roles = new[] { "Doctor" };

        // Act
        var token = _tokenService.GenerateAccessToken(user, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        var expectedExpiry = _dateTimeProvider.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes);
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ValidateAccessToken_WithValidToken_ShouldReturnPrincipal()
    {
        // Arrange
        var user = CreateTestUser();
        var roles = new[] { "Doctor" };
        var token = _tokenService.GenerateAccessToken(user, roles);

        // Act
        var principal = _tokenService.ValidateAccessToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(user.Id.ToString());
        principal.FindFirst(ClaimTypes.Email)?.Value.Should().Be(user.Email);
    }

    [Fact]
    public void ValidateAccessToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var principal = _tokenService.ValidateAccessToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateAccessToken_WithEmptyToken_ShouldReturnNull()
    {
        // Act
        var principal = _tokenService.ValidateAccessToken(string.Empty);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateAccessTokenWithExpiry_WithValidToken_ShouldReturnValidResult()
    {
        // Arrange
        var user = CreateTestUser();
        var roles = new[] { "Doctor" };
        var token = _tokenService.GenerateAccessToken(user, roles);

        // Act
        var result = _tokenService.ValidateAccessTokenWithExpiry(token);

        // Assert
        result.IsValid.Should().BeTrue();
        result.IsExpired.Should().BeFalse();
        result.Principal.Should().NotBeNull();
    }

    [Fact]
    public void ValidateAccessTokenWithExpiry_WithInvalidToken_ShouldReturnInvalidResult()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var result = _tokenService.ValidateAccessTokenWithExpiry(invalidToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.IsExpired.Should().BeFalse();
        result.Principal.Should().BeNull();
    }

    [Fact]
    public void IsTokenExpired_WithValidToken_ShouldReturnFalse()
    {
        // Arrange
        var user = CreateTestUser();
        var roles = new[] { "Doctor" };
        var token = _tokenService.GenerateAccessToken(user, roles);

        // Act
        var isExpired = _tokenService.IsTokenExpired(token);

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void GenerateAccessToken_WithNullClinicId_ShouldNotIncludeClinicIdClaim()
    {
        // Arrange
        var user = CreateTestUser();
        var roles = new[] { "SuperAdmin" };

        // Act
        var token = _tokenService.GenerateAccessToken(user, roles, null);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        jwtToken.Claims.Should().NotContain(c => c.Type == "ClinicId");
    }

    [Fact]
    public void GenerateAccessToken_WithProvidedClinicId_ShouldIncludeClinicIdClaim()
    {
        // Arrange
        var user = CreateTestUser();
        var roles = new[] { "Doctor" };
        var providedClinicId = Guid.NewGuid();

        // Act
        var token = _tokenService.GenerateAccessToken(user, roles, providedClinicId);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        jwtToken.Claims.Should().Contain(c => c.Type == "ClinicId" && c.Value == providedClinicId.ToString());
    }

    private static User CreateTestUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };
    }
}
