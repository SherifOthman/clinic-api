using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.Features.Auth.Commands.Register;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Auth.Commands;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator;
    private readonly Mock<IPhoneNumberValidationService> _phoneNumberValidationServiceMock;

    public RegisterCommandValidatorTests()
    {
        _phoneNumberValidationServiceMock = new Mock<IPhoneNumberValidationService>();
        _phoneNumberValidationServiceMock.Setup(x => x.IsValidPhoneNumber(It.IsAny<string>())).Returns(true);
        _validator = new RegisterCommandValidator(_phoneNumberValidationServiceMock.Object);
    }

    [Fact]
    public void Validate_WhenAllFieldsValid_ShouldPassValidation()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            PhoneNumber = "+1234567890"
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
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            PhoneNumber = "+1234567890"
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
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            PhoneNumber = "+1234567890"
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
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            PhoneNumber = "+1234567890"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
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
            FirstName = firstName,
            LastName = "Doe",
            Username = "johndoe",
            PhoneNumber = "+1234567890"
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
            FirstName = "John",
            LastName = lastName,
            Username = "johndoe",
            PhoneNumber = "+1234567890"
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
            FirstName = "John",
            LastName = "Doe",
            Username = username,
            PhoneNumber = "+1234567890"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WhenPhoneNumberIsInvalid_ShouldHaveValidationError(string phoneNumber)
    {
        // Arrange
        _phoneNumberValidationServiceMock.Setup(x => x.IsValidPhoneNumber(It.IsAny<string>())).Returns(false);
        
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            PhoneNumber = phoneNumber
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PhoneNumber");
    }
}