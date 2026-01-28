using ClinicManagement.Application.Features.Auth.Commands.Register;
using FluentAssertions;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Auth.Commands;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator;

    public RegisterCommandValidatorTests()
    {
        _validator = new RegisterCommandValidator();
    }

    [Fact]
    public void Validate_WhenAllFieldsValid_ShouldPassValidation()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe"
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
    public void Validate_WhenEmailIsInvalid_ShouldHaveValidationError(string email)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = email,
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe"
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
        var command = new RegisterCommand
        {
            Email = "invalid-email",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe"
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
    [InlineData("123")] // Too short
    public void Validate_WhenPasswordIsInvalid_ShouldHaveValidationError(string password)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = password,
            ConfirmPassword = password,
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_WhenPasswordsDoNotMatch_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword123!",
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConfirmPassword");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WhenFirstNameIsInvalid_ShouldHaveValidationError(string firstName)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = firstName,
            LastName = "Doe",
            Username = "johndoe"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WhenLastNameIsInvalid_ShouldHaveValidationError(string lastName)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = lastName,
            Username = "johndoe"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LastName");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WhenUsernameIsInvalid_ShouldHaveValidationError(string username)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Username = username
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
    }
}