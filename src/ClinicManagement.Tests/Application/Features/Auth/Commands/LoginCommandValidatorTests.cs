using ClinicManagement.Application.Features.Auth.Commands.Login;
using FluentAssertions;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Auth.Commands;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        _validator = new LoginCommandValidator();
    }

    [Fact]
    public void Validate_WhenAllFieldsValid_ShouldPassValidation()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WhenEmailIsEmpty_ShouldHaveValidationError(string email)
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = email,
            Password = "Password123!"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_WhenEmailFormatIsInvalid_ShouldHaveValidationError()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "invalid-email-format",
            Password = "Password123!"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WhenPasswordIsEmpty_ShouldHaveValidationError(string password)
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = password
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_WhenPasswordIsTooShort_ShouldHaveValidationError()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "123" // Too short
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}