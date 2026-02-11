using ClinicManagement.API.Infrastructure.Services;
using FluentAssertions;

namespace ClinicManagement.Tests.Services;

public class PhoneValidationServiceTests
{
    private readonly PhoneValidationService _service;

    public PhoneValidationServiceTests()
    {
        _service = new PhoneValidationService();
    }

    [Theory]
    [InlineData("+14155552671", "US")]
    [InlineData("+442071838750", "GB")]
    [InlineData("+33123456789", "FR")]
    public void ValidatePhoneNumber_WithValidInternationalNumber_ShouldReturnValid(string phoneNumber, string expectedCountry)
    {
        // Act
        var result = _service.ValidatePhoneNumber(phoneNumber);

        // Assert
        result.IsValid.Should().BeTrue();
        result.CountryCode.Should().Be(expectedCountry);
        result.FormattedNumber.Should().StartWith("+");
    }

    [Fact]
    public void ValidatePhoneNumber_WithoutPlusSign_ShouldReturnInvalid()
    {
        // Arrange
        var phoneNumber = "14155552671";

        // Act
        var result = _service.ValidatePhoneNumber(phoneNumber);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("must start with +");
    }

    [Fact]
    public void ValidatePhoneNumber_WithInvalidNumber_ShouldReturnInvalid()
    {
        // Arrange
        var phoneNumber = "+1234";

        // Act
        var result = _service.ValidatePhoneNumber(phoneNumber);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ValidatePhoneNumber_WithCountryCode_ShouldValidate()
    {
        // Arrange
        var phoneNumber = "4155552671";
        var countryCode = "US";

        // Act
        var result = _service.ValidatePhoneNumber(phoneNumber, countryCode);

        // Assert
        result.IsValid.Should().BeTrue();
        result.CountryCode.Should().Be("US");
    }
}
