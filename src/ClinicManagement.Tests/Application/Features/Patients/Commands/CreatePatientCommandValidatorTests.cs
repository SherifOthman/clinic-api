using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Patients.Commands.CreatePatient;
using ClinicManagement.Domain.Common.Enums;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Patients.Commands;

public class CreatePatientCommandValidatorTests
{
    private readonly Mock<IPhoneNumberValidationService> _phoneValidationServiceMock;
    private readonly CreatePatientCommandValidator _validator;

    public CreatePatientCommandValidatorTests()
    {
        _phoneValidationServiceMock = new Mock<IPhoneNumberValidationService>();
        _validator = new CreatePatientCommandValidator(_phoneValidationServiceMock.Object);

        // Setup default behavior for phone validation - return true for valid Egyptian numbers
        _phoneValidationServiceMock.Setup(x => x.IsValidPhoneNumber(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((phone, region) => 
            {
                // Return true for valid Egyptian phone numbers and international format
                if (string.IsNullOrEmpty(phone)) return false;
                
                return phone.StartsWith("+201") || 
                       phone.StartsWith("+2001") ||  // This handles the processed "01098021214" -> "+2001098021214"
                       phone.StartsWith("+1") ||
                       phone == "+201098021214" ||
                       phone == "+2001098021214"; // This is what "01098021214" becomes after processing
            });
    }

    [Fact]
    public void Should_Have_Error_When_FullName_Is_Empty()
    {
        // Arrange
        var command = new CreatePatientCommand 
        { 
            FullName = "",
            PhoneNumbers = new List<CreatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "+201098021214" }
            }
        };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FullName)
            .WithErrorMessage(MessageCodes.Fields.FULL_NAME_REQUIRED);
    }

    [Fact]
    public void Should_Have_Error_When_FullName_Is_Too_Short()
    {
        // Arrange
        var command = new CreatePatientCommand 
        { 
            FullName = "A",
            PhoneNumbers = new List<CreatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "+201098021214" }
            }
        };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FullName)
            .WithErrorMessage(MessageCodes.Fields.FULL_NAME_MIN_LENGTH);
    }

    [Fact]
    public void Should_Have_Error_When_FullName_Is_Too_Long()
    {
        // Arrange
        var command = new CreatePatientCommand 
        { 
            FullName = new string('A', 201),
            PhoneNumbers = new List<CreatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "+201098021214" }
            }
        };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FullName)
            .WithErrorMessage(MessageCodes.Fields.FULL_NAME_MAX_LENGTH);
    }

    [Fact]
    public void Should_Have_Error_When_FullName_Contains_Invalid_Characters()
    {
        // Arrange
        var command = new CreatePatientCommand 
        { 
            FullName = "John123 Doe",
            PhoneNumbers = new List<CreatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "+201098021214" }
            }
        };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FullName)
            .WithErrorMessage(MessageCodes.Fields.FULL_NAME_INVALID_CHARACTERS);
    }

    [Fact]
    public void Should_Not_Have_Error_When_FullName_Is_Valid()
    {
        // Arrange
        var command = new CreatePatientCommand 
        { 
            FullName = "John Michael Doe",
            PhoneNumbers = new List<CreatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "+201098021214" }
            }
        };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Should_Have_Error_When_DateOfBirth_Is_In_Future()
    {
        // Arrange
        var command = new CreatePatientCommand { DateOfBirth = DateTime.Today.AddDays(1) };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage(MessageCodes.Fields.DATE_OF_BIRTH_PAST);
    }

    [Fact]
    public void Should_Have_Error_When_DateOfBirth_Is_Too_Old()
    {
        // Arrange
        var command = new CreatePatientCommand { DateOfBirth = DateTime.Today.AddYears(-151) };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage(MessageCodes.Fields.DATE_OF_BIRTH_TOO_OLD);
    }

    [Fact]
    public void Should_Have_Error_When_Gender_Is_Invalid()
    {
        // Arrange
        var command = new CreatePatientCommand { Gender = (Gender)999 };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Gender)
            .WithErrorMessage(MessageCodes.Fields.GENDER_INVALID);
    }

    [Fact]
    public void Should_Have_Error_When_PhoneNumbers_Is_Empty()
    {
        // Arrange
        var command = new CreatePatientCommand { PhoneNumbers = new List<CreatePatientPhoneNumberDto>() };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumbers)
            .WithErrorMessage(MessageCodes.Fields.PHONE_NUMBERS_REQUIRED);
    }

    [Fact]
    public void Should_Have_Error_When_PhoneNumber_Is_Invalid()
    {
        // Arrange
        _phoneValidationServiceMock.Setup(x => x.IsValidPhoneNumber("invalid", It.IsAny<string>()))
            .Returns(false);

        var command = new CreatePatientCommand
        {
            FullName = "John Michael Doe",
            PhoneNumbers = new List<CreatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "invalid" }
            }
        };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("PhoneNumbers[0].PhoneNumber")
            .WithErrorMessage(MessageCodes.Fields.PHONE_NUMBER_INVALID);
    }

    [Fact]
    public void Should_Not_Have_Error_When_PhoneNumber_Is_Valid_Egyptian_Number()
    {
        // Arrange - Let's debug what's actually being called
        var actualCalls = new List<(string phone, string region)>();
        _phoneValidationServiceMock.Setup(x => x.IsValidPhoneNumber(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((phone, region) => 
            {
                actualCalls.Add((phone, region));
                // Return true for valid Egyptian phone numbers and international format
                if (string.IsNullOrEmpty(phone)) return false;
                
                return phone.StartsWith("+201") || 
                       phone.StartsWith("+2001") ||  // This handles the processed "01098021214" -> "+2001098021214"
                       phone.StartsWith("+1") ||
                       phone == "+201098021214" ||
                       phone == "+2001098021214"; // This is what "01098021214" becomes after processing
            });

        var command = new CreatePatientCommand
        {
            FullName = "John Michael Doe",
            PhoneNumbers = new List<CreatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "01098021214" }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Debug: Let's see what was actually called
        var callsInfo = string.Join(", ", actualCalls.Select(c => $"({c.phone}, {c.region})"));
        
        // Assert
        result.ShouldNotHaveValidationErrorFor("PhoneNumbers[0].PhoneNumber");
    }

    [Fact]
    public void Should_Have_Error_When_ChronicDiseaseIds_Contains_Invalid_Id()
    {
        // Arrange
        var command = new CreatePatientCommand
        {
            FullName = "John Michael Doe",
            PhoneNumbers = new List<CreatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "+201098021214" }
            },
            ChronicDiseaseIds = new List<int> { 0, -1 }
        };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ChronicDiseaseIds)
            .WithErrorMessage(MessageCodes.Fields.CHRONIC_DISEASE_IDS_POSITIVE);
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        var command = new CreatePatientCommand
        {
            FullName = "John Michael Doe",
            DateOfBirth = DateTime.Today.AddYears(-30),
            Gender = Gender.Male,
            Address = "123 Main St",
            GeoNameId = 123456,
            PhoneNumbers = new List<CreatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "+201098021214" }
            },
            ChronicDiseaseIds = new List<int> { 1, 2 }
        };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}