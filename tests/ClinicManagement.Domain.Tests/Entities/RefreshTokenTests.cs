using ClinicManagement.Domain.Entities;
using FluentAssertions;

namespace ClinicManagement.Domain.Tests.Entities;

public class RefreshTokenTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    // Helper — returns both the entity (Token = hash) and the raw token
    private static (RefreshToken Entity, string RawToken) Make(
        DateTimeOffset? expiry = null, string? ip = null)
        => RefreshToken.Create(Guid.NewGuid(), expiry ?? Now.AddDays(30), ip);

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_ShouldGenerateNonEmptyRawToken()
    {
        var (_, raw) = Make();
        raw.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Create_ShouldGenerateUniqueRawTokens()
    {
        var (_, raw1) = Make();
        var (_, raw2) = Make();
        raw1.Should().NotBe(raw2);
    }

    [Fact]
    public void Create_ShouldStoreHashNotRawToken()
    {
        var (entity, raw) = Make();
        // The stored Token must be the hash, not the raw value
        entity.Token.Should().NotBe(raw);
        entity.Token.Should().Be(RefreshToken.Hash(raw));
    }

    [Fact]
    public void Create_ShouldSetUserId()
    {
        var userId = Guid.NewGuid();
        var (entity, _) = RefreshToken.Create(userId, Now.AddDays(30));
        entity.UserId.Should().Be(userId);
    }

    [Fact]
    public void Create_ShouldSetExpiryTime()
    {
        var expiry = Now.AddDays(30);
        var (entity, _) = RefreshToken.Create(Guid.NewGuid(), expiry);
        entity.ExpiryTime.Should().Be(expiry);
    }

    [Fact]
    public void Create_ShouldStoreCreatedByIp_WhenProvided()
    {
        var (entity, _) = Make(ip: "192.168.1.1");
        entity.CreatedByIp.Should().Be("192.168.1.1");
    }

    [Fact]
    public void Create_ShouldHaveNullCreatedByIp_WhenNotProvided()
    {
        var (entity, _) = Make();
        entity.CreatedByIp.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldNotBeRevoked()
    {
        var (entity, _) = Make();
        entity.IsRevoked.Should().BeFalse();
    }

    // ── Hash ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Hash_ShouldBeDeterministic()
    {
        var (_, raw) = Make();
        RefreshToken.Hash(raw).Should().Be(RefreshToken.Hash(raw));
    }

    [Fact]
    public void Hash_ShouldProduceDifferentValuesForDifferentTokens()
    {
        var (_, raw1) = Make();
        var (_, raw2) = Make();
        RefreshToken.Hash(raw1).Should().NotBe(RefreshToken.Hash(raw2));
    }

    // ── IsExpired ─────────────────────────────────────────────────────────────

    [Fact]
    public void IsExpired_ShouldReturnFalse_WhenBeforeExpiry()
    {
        var (entity, _) = Make(expiry: Now.AddDays(1));
        entity.IsExpired(Now).Should().BeFalse();
    }

    [Fact]
    public void IsExpired_ShouldReturnTrue_WhenAfterExpiry()
    {
        var (entity, _) = Make(expiry: Now.AddDays(-1));
        entity.IsExpired(Now).Should().BeTrue();
    }

    [Fact]
    public void IsExpired_ShouldReturnTrue_WhenExactlyAtExpiry()
    {
        var (entity, _) = Make(expiry: Now);
        entity.IsExpired(Now).Should().BeTrue();
    }

    // ── IsActive ──────────────────────────────────────────────────────────────

    [Fact]
    public void IsActive_ShouldReturnTrue_WhenNotRevokedAndNotExpired()
    {
        var (entity, _) = Make(expiry: Now.AddDays(1));
        entity.IsActive(Now).Should().BeTrue();
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenExpired()
    {
        var (entity, _) = Make(expiry: Now.AddDays(-1));
        entity.IsActive(Now).Should().BeFalse();
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenRevoked()
    {
        var (entity, _) = Make(expiry: Now.AddDays(1));
        entity.Revoke("1.2.3.4", Now);
        entity.IsActive(Now).Should().BeFalse();
    }

    // ── Revoke ────────────────────────────────────────────────────────────────

    [Fact]
    public void Revoke_ShouldSetIsRevoked()
    {
        var (entity, _) = Make();
        entity.Revoke("1.2.3.4", Now);
        entity.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void Revoke_ShouldStampRevokedAt()
    {
        var (entity, _) = Make();
        entity.Revoke("1.2.3.4", Now);
        entity.RevokedAt.Should().Be(Now);
    }

    [Fact]
    public void Revoke_ShouldStoreRevokedByIp()
    {
        var (entity, _) = Make();
        entity.Revoke("1.2.3.4", Now);
        entity.RevokedByIp.Should().Be("1.2.3.4");
    }

    [Fact]
    public void Revoke_ShouldStoreHashOfReplacedByToken_WhenProvided()
    {
        var (entity, _) = Make();
        var (_, newRaw) = Make();
        entity.Revoke("1.2.3.4", Now, newRaw);
        // ReplacedByToken stores the hash, not the raw value
        entity.ReplacedByToken.Should().Be(RefreshToken.Hash(newRaw));
        entity.ReplacedByToken.Should().NotBe(newRaw);
    }

    [Fact]
    public void Revoke_ShouldLeaveReplacedByTokenNull_WhenNotProvided()
    {
        var (entity, _) = Make();
        entity.Revoke("1.2.3.4", Now);
        entity.ReplacedByToken.Should().BeNull();
    }
}
