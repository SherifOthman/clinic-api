using ClinicManagement.Domain.Entities;
using FluentAssertions;

namespace ClinicManagement.Domain.Tests.Entities;

public class UserTokenTests
{
    [Fact]
    public void IsExpired_ShouldReturnFalse_WhenTokenNotExpired()
    {
        // Arrange
        var now = new DateTime(2026, 2, 24, 12, 0, 0, DateTimeKind.Utc);
        var token = new UserToken
        {
            ExpiresAt = now.AddMinutes(10)
        };

        // Act
        var result = token.IsExpired(now);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_ShouldReturnTrue_WhenTokenExpired()
    {
        // Arrange
        var now = new DateTime(2026, 2, 24, 12, 0, 0, DateTimeKind.Utc);
        var token = new UserToken
        {
            ExpiresAt = now.AddMinutes(-10)
        };

        // Act
        var result = token.IsExpired(now);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenNotUsedAndNotExpired()
    {
        // Arrange
        var now = new DateTime(2026, 2, 24, 12, 0, 0, DateTimeKind.Utc);
        var token = new UserToken
        {
            ExpiresAt = now.AddMinutes(10),
            IsUsed = false
        };

        // Act
        var result = token.IsValid(now);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenTokenUsed()
    {
        // Arrange
        var now = new DateTime(2026, 2, 24, 12, 0, 0, DateTimeKind.Utc);
        var token = new UserToken
        {
            ExpiresAt = now.AddMinutes(10),
            IsUsed = true
        };

        // Act
        var result = token.IsValid(now);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenTokenExpired()
    {
        // Arrange
        var now = new DateTime(2026, 2, 24, 12, 0, 0, DateTimeKind.Utc);
        var token = new UserToken
        {
            ExpiresAt = now.AddMinutes(-10),
            IsUsed = false
        };

        // Act
        var result = token.IsValid(now);

        // Assert
        result.Should().BeFalse();
    }
}
