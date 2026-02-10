using ClinicManagement.Domain.Common.Exceptions;
using ClinicManagement.Domain.Common.ValueObjects;
using FluentAssertions;

namespace ClinicManagement.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("john@example.com")]
    [InlineData("jane.doe@company.co.uk")]
    [InlineData("user+tag@domain.com")]
    [InlineData("test_user@sub.domain.org")]
    public void Constructor_ValidEmail_ShouldCreateEmail(string emailValue)
    {
        // Act
        var email = new Email(emailValue);

        // Assert
        email.Value.Should().Be(emailValue.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_EmptyEmail_ShouldThrowException(string emailValue)
    {
        // Act
        Action act = () => new Email(emailValue);

        // Assert
        act.Should().Throw<InvalidEmailException>()
            .WithMessage("Email cannot be empty");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user @example.com")]
    [InlineData("user@.com")]
    public void Constructor_InvalidFormat_ShouldThrowException(string emailValue)
    {
        // Act
        Action act = () => new Email(emailValue);

        // Assert
        act.Should().Throw<InvalidEmailException>()
            .WithMessage("Email format is invalid");
    }

    [Fact]
    public void Constructor_TooLong_ShouldThrowException()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@example.com";

        // Act
        Action act = () => new Email(longEmail);

        // Assert
        act.Should().Throw<InvalidEmailException>()
            .WithMessage("Email cannot exceed 255 characters");
    }

    [Fact]
    public void Constructor_ShouldNormalizeToLowerCase()
    {
        // Arrange
        var email = "John.Doe@EXAMPLE.COM";

        // Act
        var result = new Email(email);

        // Assert
        result.Value.Should().Be("john.doe@example.com");
    }

    [Fact]
    public void Domain_ShouldReturnCorrectDomain()
    {
        // Arrange
        var email = new Email("john@example.com");

        // Act
        var domain = email.Domain;

        // Assert
        domain.Should().Be("example.com");
    }

    [Fact]
    public void LocalPart_ShouldReturnCorrectLocalPart()
    {
        // Arrange
        var email = new Email("john.doe@example.com");

        // Act
        var localPart = email.LocalPart;

        // Assert
        localPart.Should().Be("john.doe");
    }

    [Theory]
    [InlineData("john@example.com", "example.com", true)]
    [InlineData("john@example.com", "EXAMPLE.COM", true)]
    [InlineData("john@example.com", "other.com", false)]
    public void IsFromDomain_ShouldCheckDomainCorrectly(string emailValue, string domain, bool expected)
    {
        // Arrange
        var email = new Email(emailValue);

        // Act
        var result = email.IsFromDomain(domain);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Equals_SameValue_ShouldBeEqual()
    {
        // Arrange
        var email1 = new Email("john@example.com");
        var email2 = new Email("john@example.com");

        // Act & Assert
        email1.Should().Be(email2);
        (email1 == email2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        var email1 = new Email("john@example.com");
        var email2 = new Email("jane@example.com");

        // Act & Assert
        email1.Should().NotBe(email2);
        (email1 != email2).Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var email = new Email("john@example.com");

        // Act
        var result = email.ToString();

        // Assert
        result.Should().Be("john@example.com");
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        // Arrange
        var email = new Email("john@example.com");

        // Act
        string value = email;

        // Assert
        value.Should().Be("john@example.com");
    }

    [Fact]
    public void ExplicitConversion_FromString_ShouldWork()
    {
        // Arrange
        var emailString = "john@example.com";

        // Act
        var email = (Email)emailString;

        // Assert
        email.Value.Should().Be("john@example.com");
    }
}
