using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;
using ClinicManagement.Domain.Common.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Patients.Commands;

public class UpdatePatientCommandValidatorTests
{
    private readonly Mock<IPhoneNumberValidationService> _phoneValidationServiceMock;
    private readonly UpdatePatientCommandValidator _validator;

    public UpdatePatientCommandValidatorTests()
    {
        _phoneValidationServiceMock = new Mock<IPhoneNumberValidationService>();
        _phoneValidationServiceMock.Setup(x => x.IsValidPhoneNumber(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        
        _validator = new UpdatePatientCommandValidator(_phoneValidationServiceMock.Object);
    }

    [Fact]
    public void Validate_WhenAllFieldsValid_ShouldPassValidation()
    {
        // Arrange
        var command = new UpdatePatientCommand
        {
            Id = 1,
            FullName = "John Doe",
            DateOfBirth = DateTime.Today.AddYears(-30),
            Gender = Gender.Male,
            PhoneNumbers = new List<UpdatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "+201098021214" }
            },
            ChronicDiseaseIds = new List<int> { 1, 2 }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenIdIsInvalid_ShouldHaveValidationError(int id)
    {
        // Arrange
        var command = new UpdatePatientCommand
        {
            Id = id,
            FullName = "John Doe",
            PhoneNumbers = new List<UpdatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "+201098021214" }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("A")] // Too short
    public void Validate_WhenFullNameIsInvalid_ShouldHaveValidationError(string fullName)
    {
        // Arrange
        var command = new UpdatePatientCommand
        {
            Id = 1,
            FullName = fullName,
            PhoneNumbers = new List<UpdatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "+201098021214" }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }

    [Fact]
    public void Validate_WhenFullNameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdatePatientCommand
        {
            Id = 1,
            FullName = new string('A', 201), // Too long
            PhoneNumbers = new List<UpdatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "+201098021214" }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }

    [Theory]
    [InlineData("John123")] // Contains numbers
    [InlineData("John@Doe")] // Contains special characters
    public void Validate_WhenFullNameHasInvalidCharacters_ShouldHaveValidationError(string fullName)
    {
        // Arrange
        var command = new UpdatePatientCommand
        {
            Id = 1,
            FullName = fullName,
            PhoneNumbers = new List<UpdatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "+201098021214" }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }

    [Fact]
    public void Validate_WhenDateOfBirthInFuture_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdatePatientCommand
        {
            Id = 1,
            FullName = "John Doe",
            DateOfBirth = DateTime.Today.AddDays(1), // Future date
            PhoneNumbers = new List<UpdatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "+201098021214" }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DateOfBirth");
    }

    [Fact]
    public void Validate_WhenDateOfBirthTooOld_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdatePatientCommand
        {
            Id = 1,
            FullName = "John Doe",
            DateOfBirth = DateTime.Today.AddYears(-151), // Too old
            PhoneNumbers = new List<UpdatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "+201098021214" }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DateOfBirth");
    }

    [Fact]
    public void Validate_WhenPhoneNumbersEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdatePatientCommand
        {
            Id = 1,
            FullName = "John Doe",
            PhoneNumbers = new List<UpdatePatientPhoneNumberDto>()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PhoneNumbers");
    }

    [Fact]
    public void Validate_WhenPhoneNumberInvalid_ShouldHaveValidationError()
    {
        // Arrange
        _phoneValidationServiceMock.Setup(x => x.IsValidPhoneNumber(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        var command = new UpdatePatientCommand
        {
            Id = 1,
            FullName = "John Doe",
            PhoneNumbers = new List<UpdatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "invalid-phone" }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("PhoneNumber"));
    }

    [Fact]
    public void Validate_WhenChronicDiseaseIdsContainZero_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdatePatientCommand
        {
            Id = 1,
            FullName = "John Doe",
            PhoneNumbers = new List<UpdatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "+201098021214" }
            },
            ChronicDiseaseIds = new List<int> { 1, 0, 2 } // Contains zero
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ChronicDiseaseIds");
    }

    [Theory]
    [InlineData("محمد أحمد")] // Arabic name
    [InlineData("John O'Connor")] // Name with apostrophe
    [InlineData("Mary-Jane Smith")] // Name with hyphen
    public void Validate_WhenFullNameHasValidCharacters_ShouldPassValidation(string fullName)
    {
        // Arrange
        var command = new UpdatePatientCommand
        {
            Id = 1,
            FullName = fullName,
            PhoneNumbers = new List<UpdatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "+201098021214" }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}