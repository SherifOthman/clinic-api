using ClinicManagement.API.Common.Options;
using ClinicManagement.API.Entities;
using ClinicManagement.API.Infrastructure.Data;
using ClinicManagement.API.Infrastructure.Services;
using ClinicManagement.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace ClinicManagement.Tests.Services;

public class RefreshTokenServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly RefreshTokenService _service;
    private readonly DateTimeProvider _dateTimeProvider;
    private readonly JwtOptions _jwtOptions;
    private readonly Guid _testUserId;

    public RefreshTokenServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _testUserId = Guid.NewGuid();
        var httpContextAccessor = new TestHttpContextAccessor(Guid.NewGuid());
        var currentUserService = new CurrentUserService(httpContextAccessor);
        _dateTimeProvider = new DateTimeProvider();

        _context = new ApplicationDbContext(options, currentUserService, _dateTimeProvider);

        _jwtOptions = new JwtOptions
        {
            RefreshTokenExpirationDays = 7
        };

        var logger = Mock.Of<ILogger<RefreshTokenService>>();
        _service = new RefreshTokenService(
            _context,
            currentUserService,
            _dateTimeProvider,
            Options.Create(_jwtOptions),
            logger);
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_ShouldCreateToken()
    {
        // Act
        var token = await _service.GenerateRefreshTokenAsync(_testUserId, "192.168.1.1");

        // Assert
        token.Should().NotBeNull();
        token.Token.Should().NotBeNullOrEmpty();
        token.UserId.Should().Be(_testUserId);
        token.CreatedByIp.Should().Be("192.168.1.1");
        token.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_ShouldSetExpiryDate()
    {
        // Act
        var token = await _service.GenerateRefreshTokenAsync(_testUserId);

        // Assert
        var expectedExpiry = _dateTimeProvider.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays);
        token.ExpiryTime.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_ShouldGenerateUniqueTokens()
    {
        // Act
        var token1 = await _service.GenerateRefreshTokenAsync(_testUserId);
        var token2 = await _service.GenerateRefreshTokenAsync(_testUserId);

        // Assert
        token1.Token.Should().NotBe(token2.Token);
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_ShouldSaveToDatabase()
    {
        // Act
        var token = await _service.GenerateRefreshTokenAsync(_testUserId);

        // Assert
        var savedToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token.Token);
        savedToken.Should().NotBeNull();
        savedToken!.UserId.Should().Be(_testUserId);
    }

    [Fact]
    public async Task GetActiveRefreshTokenAsync_WithValidToken_ShouldReturnToken()
    {
        // Arrange
        // Create a test user first
        var testUser = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            UserName = "testuser"
        };
        _context.Users.Add(testUser);
        await _context.SaveChangesAsync();
        
        var token = await _service.GenerateRefreshTokenAsync(_testUserId);

        // Act
        var retrievedToken = await _service.GetActiveRefreshTokenAsync(token.Token);

        // Assert
        retrievedToken.Should().NotBeNull();
        retrievedToken!.Token.Should().Be(token.Token);
        retrievedToken.UserId.Should().Be(_testUserId);
    }

    [Fact]
    public async Task GetActiveRefreshTokenAsync_WithRevokedToken_ShouldReturnNull()
    {
        // Arrange
        var token = await _service.GenerateRefreshTokenAsync(_testUserId);
        await _service.RevokeRefreshTokenAsync(token.Token);

        // Act
        var retrievedToken = await _service.GetActiveRefreshTokenAsync(token.Token);

        // Assert
        retrievedToken.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveRefreshTokenAsync_WithNonExistentToken_ShouldReturnNull()
    {
        // Act
        var retrievedToken = await _service.GetActiveRefreshTokenAsync("non-existent-token");

        // Assert
        retrievedToken.Should().BeNull();
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_ShouldMarkTokenAsRevoked()
    {
        // Arrange
        var testUser = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            UserName = "testuser"
        };
        _context.Users.Add(testUser);
        await _context.SaveChangesAsync();
        
        var token = await _service.GenerateRefreshTokenAsync(_testUserId);

        // Act
        await _service.RevokeRefreshTokenAsync(token.Token, "192.168.1.1");

        // Assert
        var revokedToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token.Token);
        revokedToken.Should().NotBeNull();
        revokedToken!.IsRevoked.Should().BeTrue();
        revokedToken.RevokedAt.Should().NotBeNull();
        revokedToken.RevokedByIp.Should().Be("192.168.1.1");
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_WithReplacementToken_ShouldSetReplacedByToken()
    {
        // Arrange
        var testUser = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            UserName = "testuser"
        };
        _context.Users.Add(testUser);
        await _context.SaveChangesAsync();
        
        var token = await _service.GenerateRefreshTokenAsync(_testUserId);
        var replacementToken = "new-token-123";

        // Act
        await _service.RevokeRefreshTokenAsync(token.Token, replacedByToken: replacementToken);

        // Assert
        var revokedToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token.Token);
        revokedToken!.ReplacedByToken.Should().Be(replacementToken);
    }

    [Fact]
    public async Task RevokeAllUserRefreshTokensAsync_ShouldRevokeAllActiveTokens()
    {
        // Arrange
        var token1 = await _service.GenerateRefreshTokenAsync(_testUserId);
        var token2 = await _service.GenerateRefreshTokenAsync(_testUserId);
        var token3 = await _service.GenerateRefreshTokenAsync(_testUserId);

        // Act
        await _service.RevokeAllUserRefreshTokensAsync(_testUserId, "192.168.1.1");

        // Assert
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == _testUserId && !rt.IsRevoked)
            .ToListAsync();
        
        activeTokens.Should().BeEmpty();
    }

    [Fact]
    public async Task RevokeAllUserRefreshTokensAsync_ShouldNotAffectOtherUsers()
    {
        // Arrange
        var testUser = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            UserName = "testuser"
        };
        var otherUserId = Guid.NewGuid();
        var otherUser = new User
        {
            Id = otherUserId,
            Email = "other@example.com",
            FirstName = "Other",
            LastName = "User",
            UserName = "otheruser"
        };
        _context.Users.AddRange(testUser, otherUser);
        await _context.SaveChangesAsync();
        
        var userToken = await _service.GenerateRefreshTokenAsync(_testUserId);
        var otherUserToken = await _service.GenerateRefreshTokenAsync(otherUserId);

        // Act
        await _service.RevokeAllUserRefreshTokensAsync(_testUserId);

        // Assert
        var otherUserActiveToken = await _service.GetActiveRefreshTokenAsync(otherUserToken.Token);
        otherUserActiveToken.Should().NotBeNull();
    }

    [Fact]
    public async Task CleanupExpiredTokensAsync_ShouldRemoveExpiredTokens()
    {
        // Arrange
        var expiredToken = new RefreshToken
        {
            Token = "expired-token",
            UserId = _testUserId,
            ExpiryTime = _dateTimeProvider.UtcNow.AddDays(-1),
            CreatedAt = _dateTimeProvider.UtcNow.AddDays(-8),
            CreatedByIp = "192.168.1.1"
        };
        _context.RefreshTokens.Add(expiredToken);
        await _context.SaveChangesAsync();

        // Act
        var count = await _service.CleanupExpiredTokensAsync();

        // Assert
        count.Should().Be(1);
        var tokenExists = await _context.RefreshTokens.AnyAsync(rt => rt.Token == "expired-token");
        tokenExists.Should().BeFalse();
    }

    [Fact]
    public async Task CleanupExpiredTokensAsync_ShouldRemoveRevokedTokens()
    {
        // Arrange
        var testUser = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            UserName = "testuser"
        };
        _context.Users.Add(testUser);
        await _context.SaveChangesAsync();
        
        var token = await _service.GenerateRefreshTokenAsync(_testUserId);
        await _service.RevokeRefreshTokenAsync(token.Token);

        // Act
        var count = await _service.CleanupExpiredTokensAsync();

        // Assert
        count.Should().Be(1);
        var tokenExists = await _context.RefreshTokens.AnyAsync(rt => rt.Token == token.Token);
        tokenExists.Should().BeFalse();
    }

    [Fact]
    public async Task CleanupExpiredTokensAsync_ShouldNotRemoveActiveTokens()
    {
        // Arrange
        var activeToken = await _service.GenerateRefreshTokenAsync(_testUserId);

        // Act
        var count = await _service.CleanupExpiredTokensAsync();

        // Assert
        count.Should().Be(0);
        var tokenExists = await _context.RefreshTokens.AnyAsync(rt => rt.Token == activeToken.Token);
        tokenExists.Should().BeTrue();
    }

    [Fact]
    public async Task CleanupExpiredTokensAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var expiredToken1 = new RefreshToken
        {
            Token = "expired-1",
            UserId = _testUserId,
            ExpiryTime = _dateTimeProvider.UtcNow.AddDays(-1),
            CreatedAt = _dateTimeProvider.UtcNow.AddDays(-8),
            CreatedByIp = "192.168.1.1"
        };
        var expiredToken2 = new RefreshToken
        {
            Token = "expired-2",
            UserId = _testUserId,
            ExpiryTime = _dateTimeProvider.UtcNow.AddDays(-2),
            CreatedAt = _dateTimeProvider.UtcNow.AddDays(-9),
            CreatedByIp = "192.168.1.1"
        };
        _context.RefreshTokens.AddRange(expiredToken1, expiredToken2);
        await _context.SaveChangesAsync();

        // Act
        var count = await _service.CleanupExpiredTokensAsync();

        // Assert
        count.Should().Be(2);
    }
}
