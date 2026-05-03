using ClinicManagement.Domain.Entities;
using FluentAssertions;

namespace ClinicManagement.Domain.Tests.Entities;

public class RefreshTokenTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_ShouldGenerateNonEmptyToken()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Now.AddDays(30));
        token.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Create_ShouldGenerateUniqueTokens()
    {
        var userId = Guid.NewGuid();
        var t1 = RefreshToken.Create(userId, Now.AddDays(30));
        var t2 = RefreshToken.Create(userId, Now.AddDays(30));
        t1.Token.Should().NotBe(t2.Token);
    }

    [Fact]
    public void Create_ShouldSetUserId()
    {
        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(userId, Now.AddDays(30));
        token.UserId.Should().Be(userId);
    }

    [Fact]
    public void Create_ShouldSetExpiryTime()
    {
        var expiry = Now.AddDays(30);
        var token = RefreshToken.Create(Guid.NewGuid(), expiry);
        token.ExpiryTime.Should().Be(expiry);
    }

    [Fact]
    public void Create_ShouldStoreCreatedByIp_WhenProvided()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Now.AddDays(30), "192.168.1.1");
        token.CreatedByIp.Should().Be("192.168.1.1");
    }

    [Fact]
    public void Create_ShouldHaveNullCreatedByIp_WhenNotProvided()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Now.AddDays(30));
        token.CreatedByIp.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldNotBeRevoked()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Now.AddDays(30));
        token.IsRevoked.Should().BeFalse();
    }

    // ── IsExpired ─────────────────────────────────────────────────────────────

    [Fact]
    public void IsExpired_ShouldReturnFalse_WhenBeforeExpiry()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Now.AddDays(1));
        token.IsExpired(Now).Should().BeFalse();
    }

    [Fact]
    public void IsExpired_ShouldReturnTrue_WhenAfterExpiry()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Now.AddDays(-1));
        token.IsExpired(Now).Should().BeTrue();
    }

    [Fact]
    public void IsExpired_ShouldReturnTrue_WhenExactlyAtExpiry()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Now);
        token.IsExpired(Now).Should().BeTrue();
    }

    // ── IsActive ──────────────────────────────────────────────────────────────

    [Fact]
    public void IsActive_ShouldReturnTrue_WhenNotRevokedAndNotExpired()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Now.AddDays(1));
        token.IsActive(Now).Should().BeTrue();
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenExpired()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Now.AddDays(-1));
        token.IsActive(Now).Should().BeFalse();
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenRevoked()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Now.AddDays(1));
        token.Revoke("1.2.3.4", Now);
        token.IsActive(Now).Should().BeFalse();
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenRevokedAndExpired()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Now.AddDays(-1));
        token.Revoke("1.2.3.4", Now);
        token.IsActive(Now).Should().BeFalse();
    }

    // ── Revoke ────────────────────────────────────────────────────────────────

    [Fact]
    public void Revoke_ShouldSetIsRevoked()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Now.AddDays(1));
        token.Revoke("1.2.3.4", Now);
        token.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void Revoke_ShouldStampRevokedAt()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Now.AddDays(1));
        token.Revoke("1.2.3.4", Now);
        token.RevokedAt.Should().Be(Now);
    }

    [Fact]
    public void Revoke_ShouldStoreRevokedByIp()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Now.AddDays(1));
        token.Revoke("1.2.3.4", Now);
        token.RevokedByIp.Should().Be("1.2.3.4");
    }

    [Fact]
    public void Revoke_ShouldStoreReplacedByToken_WhenProvided()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Now.AddDays(1));
        token.Revoke("1.2.3.4", Now, "new-token-xyz");
        token.ReplacedByToken.Should().Be("new-token-xyz");
    }

    [Fact]
    public void Revoke_ShouldLeaveReplacedByTokenNull_WhenNotProvided()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), Now.AddDays(1));
        token.Revoke("1.2.3.4", Now);
        token.ReplacedByToken.Should().BeNull();
    }
}
