using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.Features.Auth.Commands.CompleteOnboarding;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Auth.Commands;

public class CompleteOnboardingCommandValidatorTests
{
    private readonly Mock<IPhoneNumberValidationService> _phoneValidationServiceMock;
    private readonly CompleteOnboardingCommandValidator _validator;

    public CompleteOnboardingCommandValidatorTests()
    {
        _phoneValidationServiceMock = new Mock<IPhoneNumberValidationService>();
        _phoneValidationServiceMock.Setup(x => x.IsValidPhoneNumber(It.IsAny<string>()))
            .Returns(true);
        
        _validator = new CompleteOnboardingCommandValidator(_phoneValidationServiceMock.Object);
    }

    [Fact]
    public void Validate_WhenAllFieldsValid_ShouldPassValidation()
    {
        // Arrange
        var command = new CompleteOnboardingCommand(
            "Test Clinic",
            1,
            "Main Branch",
            "123 Main Street",
            1,
            2,
            3,
            new List<BranchPhoneNumberDto>
            {
                new("+201098021214", "Main")
            }
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WhenClinicNameIsEmpty_ShouldHaveValidationError(string clinicName)
    {
        // Arrange
        var command = new CompleteOnboardingCommand(
            clinicName,
            1,
            "Main Branch",
            "123 Main Street",
            1,
            2,
            3,
            new List<BranchPhoneNumberDto>
            {
                new("+201098021214")
            }
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ClinicName");
    }

    [Fact]
    public void Validate_WhenClinicNameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CompleteOnboardingCommand(
            new string('A', 101), // Too long
            1,
            "Main Branch",
            "123 Main Street",
            1,
            2,
            3,
            new List<BranchPhoneNumberDto>
            {
                new("+201098021214")
            }
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ClinicName");
    }

    [Theory]
    [InlineData("Clinic@Name")] // Contains @
    [InlineData("Clinic#Name")] // Contains #
    public void Validate_WhenClinicNameHasInvalidCharacters_ShouldHaveValidationError(string clinicName)
    {
        // Arrange
        var command = new CompleteOnboardingCommand(
            clinicName,
            1,
            "Main Branch",
            "123 Main Street",
            1,
            2,
            3,
            new List<BranchPhoneNumberDto>
            {
                new("+201098021214")
            }
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ClinicName");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenSubscriptionPlanIdInvalid_ShouldHaveValidationError(int subscriptionPlanId)
    {
        // Arrange
        var command = new CompleteOnboardingCommand(
            "Test Clinic",
            subscriptionPlanId,
            "Main Branch",
            "123 Main Street",
            1,
            2,
            3,
            new List<BranchPhoneNumberDto>
            {
                new("+201098021214")
            }
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SubscriptionPlanId");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WhenBranchNameIsEmpty_ShouldHaveValidationError(string branchName)
    {
        // Arrange
        var command = new CompleteOnboardingCommand(
            "Test Clinic",
            1,
            branchName,
            "123 Main Street",
            1,
            2,
            3,
            new List<BranchPhoneNumberDto>
            {
                new("+201098021214")
            }
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "BranchName");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WhenBranchAddressIsEmpty_ShouldHaveValidationError(string branchAddress)
    {
        // Arrange
        var command = new CompleteOnboardingCommand(
            "Test Clinic",
            1,
            "Main Branch",
            branchAddress,
            1,
            2,
            3,
            new List<BranchPhoneNumberDto>
            {
                new("+201098021214")
            }
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "BranchAddress");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenCountryIdInvalid_ShouldHaveValidationError(int countryId)
    {
        // Arrange
        var command = new CompleteOnboardingCommand(
            "Test Clinic",
            1,
            "Main Branch",
            "123 Main Street",
            countryId,
            2,
            3,
            new List<BranchPhoneNumberDto>
            {
                new("+201098021214")
            }
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CountryId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenCityIdInvalid_ShouldHaveValidationError(int cityId)
    {
        // Arrange
        var command = new CompleteOnboardingCommand(
            "Test Clinic",
            1,
            "Main Branch",
            "123 Main Street",
            1,
            2,
            cityId,
            new List<BranchPhoneNumberDto>
            {
                new("+201098021214")
            }
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CityId");
    }

    [Fact]
    public void Validate_WhenBranchPhoneNumbersEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CompleteOnboardingCommand(
            "Test Clinic",
            1,
            "Main Branch",
            "123 Main Street",
            1,
            2,
            3,
            new List<BranchPhoneNumberDto>()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "BranchPhoneNumbers");
    }

    [Fact]
    public void Validate_WhenPhoneNumberInvalid_ShouldHaveValidationError()
    {
        // Arrange
        _phoneValidationServiceMock.Setup(x => x.IsValidPhoneNumber(It.IsAny<string>()))
            .Returns(false);

        var command = new CompleteOnboardingCommand(
            "Test Clinic",
            1,
            "Main Branch",
            "123 Main Street",
            1,
            2,
            3,
            new List<BranchPhoneNumberDto>
            {
                new("invalid-phone")
            }
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("PhoneNumber"));
    }

    [Theory]
    [InlineData("عيادة الأسنان")] // Arabic clinic name
    [InlineData("Dr. Smith's Clinic")] // Name with apostrophe
    [InlineData("Health Center 123")] // Name with numbers
    public void Validate_WhenClinicNameHasValidCharacters_ShouldPassValidation(string clinicName)
    {
        // Arrange
        var command = new CompleteOnboardingCommand(
            clinicName,
            1,
            "Main Branch",
            "123 Main Street",
            1,
            2,
            3,
            new List<BranchPhoneNumberDto>
            {
                new("+201098021214")
            }
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenPhoneLabelTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CompleteOnboardingCommand(
            "Test Clinic",
            1,
            "Main Branch",
            "123 Main Street",
            1,
            2,
            3,
            new List<BranchPhoneNumberDto>
            {
                new("+201098021214", new string('A', 51)) // Too long
            }
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Label"));
    }
}