using ClinicManagement.Domain.Entities;
using FluentAssertions;

namespace ClinicManagement.Domain.Tests.Entities;

public class UserTokenTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void IsExpired_ShouldReturnFalse_WhenTokenNotExpired() =>
        new UserToken { ExpiresAt = Now.AddMinutes(10) }.IsExpired(Now).Should().BeFalse();

    [Fact]
    public void IsExpired_ShouldReturnTrue_WhenTokenExpired() =>
        new UserToken { ExpiresAt = Now.AddMinutes(-10) }.IsExpired(Now).Should().BeTrue();

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenNotUsedAndNotExpired() =>
        new UserToken { ExpiresAt = Now.AddMinutes(10), IsUsed = false }.IsValid(Now).Should().BeTrue();

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenTokenIsUsed() =>
        new UserToken { ExpiresAt = Now.AddMinutes(10), IsUsed = true }.IsValid(Now).Should().BeFalse();

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenTokenIsExpired() =>
        new UserToken { ExpiresAt = Now.AddMinutes(-10), IsUsed = false }.IsValid(Now).Should().BeFalse();
}
