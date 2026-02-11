using ClinicManagement.API.Common.Extensions;
using FluentAssertions;

namespace ClinicManagement.Tests.Extensions;

public class DateTimeExtensionsTests
{
    [Fact]
    public void CalculateAge_ShouldReturnCorrectAge()
    {
        // Arrange
        var dateOfBirth = new DateTime(1990, 1, 1);
        var expectedAge = DateTime.UtcNow.Year - 1990;
        
        // Adjust if birthday hasn't occurred this year
        if (DateTime.UtcNow.DayOfYear < dateOfBirth.DayOfYear)
            expectedAge--;

        // Act
        var age = dateOfBirth.CalculateAge();

        // Assert
        age.Should().Be(expectedAge);
    }

    [Fact]
    public void CalculateAge_ForBirthdayToday_ShouldReturnCorrectAge()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow.AddYears(-25);

        // Act
        var age = dateOfBirth.CalculateAge();

        // Assert
        age.Should().Be(25);
    }

    [Fact]
    public void GetDateForAge_ShouldReturnCorrectDate()
    {
        // Arrange
        var targetAge = 30;

        // Act
        var date = DateTimeExtensions.GetDateForAge(targetAge);

        // Assert
        var calculatedAge = date.CalculateAge();
        calculatedAge.Should().BeInRange(targetAge - 1, targetAge + 1);
    }

    [Fact]
    public void GetMaxDateOfBirthForMinAge_ShouldReturnCorrectDate()
    {
        // Arrange
        var minAge = 18;

        // Act
        var maxDate = DateTimeExtensions.GetMaxDateOfBirthForMinAge(minAge);

        // Assert
        var age = maxDate.CalculateAge();
        age.Should().BeGreaterThanOrEqualTo(minAge);
    }
}
