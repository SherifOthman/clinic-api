using ClinicManagement.Application.Common.Services;
using FluentAssertions;
using Xunit;

namespace ClinicManagement.Tests.Application.Services;

public class PhoneNumberValidationServiceTests
{
    private readonly PhoneNumberValidationService _service;

    public PhoneNumberValidationServiceTests()
    {
        _service = new PhoneNumberValidationService();
    }

    [Theory]
    [InlineData("+201098021214", true)] // Valid Egyptian number with country code
    [InlineData("01098021214", true)]   // Valid Egyptian number without country code (11 digits)
    [InlineData("1098021214", true)]    // Valid Egyptian number without country code (10 digits)
    [InlineData("+12125551234", true)]  // Valid US number
    [InlineData("123456789", false)]    // Too short
    [InlineData("", false)]             // Empty
    [InlineData("abc123", false)]       // Invalid format
    public void IsValidPhoneNumber_ShouldValidateCorrectly(string phoneNumber, bool expectedResult)
    {
        // Act
        var result = _service.IsValidPhoneNumber(phoneNumber, "EG");

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("01098021214", "+201098021214")] // Egyptian number starting with 01
    [InlineData("1098021214", "+201098021214")]  // Egyptian number starting with 1
    [InlineData("+201098021214", "+201098021214")] // Already formatted
    public void GetE164Format_WithEgyptianNumbers_ShouldFormatCorrectly(string input, string expected)
    {
        // Act
        var result = _service.GetE164Format(input, "EG");

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ValidatePhoneNumber_WithValidEgyptianNumber_ShouldReturnDetailedResult()
    {
        // Arrange
        var phoneNumber = "01098021214";

        // Act
        var result = _service.ValidatePhoneNumber(phoneNumber, "EG");

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.CountryCode.Should().Be("20");
        result.RegionCode.Should().Be("EG");
        result.E164Format.Should().Be("+201098021214");
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void ValidatePhoneNumber_WithInvalidNumber_ShouldReturnError()
    {
        // Arrange
        var phoneNumber = "123";

        // Act
        var result = _service.ValidatePhoneNumber(phoneNumber, "EG");

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ValidatePhoneNumber_WithEmptyNumber_ShouldReturnError()
    {
        // Arrange
        var phoneNumber = "";

        // Act
        var result = _service.ValidatePhoneNumber(phoneNumber, "EG");

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Phone number cannot be empty");
    }

    [Theory]
    [InlineData("+201098021214")] // Egyptian number international format
    [InlineData("+12125551234")]  // US number international format
    public void GetInternationalFormat_ShouldFormatCorrectly(string input)
    {
        // Act
        var result = _service.GetInternationalFormat(input, "EG");

        // Assert
        result.Should().NotBeNullOrEmpty();
        // The international format should contain the country code and be formatted
        result.Should().Contain("+");
    }
}