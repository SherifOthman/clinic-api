using ClinicManagement.Domain.Entities;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicManagement.Domain.Tests.Entities;

public class UserTokenTests
{
    [Fact]
    public void IsExpired_ShouldReturnFalse_WhenTokenNoExpired()
    {
        var token = new UserToken
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        token.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_ShouldReturnTrue_WhenTokenNoExpired()
    {
        var token = new UserToken
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(-10)
        };

        token.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenNotUsedAndNotExpired()
    {
        var token = new UserToken
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };

        token.IsValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenTokenUsed()
    {
        var token = new UserToken
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = true
        };

        token.IsValid.Should().BeFalse();
    }


    [Fact]
    public void IsValid_ShouldReturnFalse_WhenTokenExpired()
    {
        var token = new UserToken
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(-10),
            IsUsed = false
        };
  
        

        token.IsValid.Should().BeFalse();
    }
}
