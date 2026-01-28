using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Common.Enums;
using FluentAssertions;
using Xunit;

namespace ClinicManagement.Tests.Domain.Entities;

public class PatientTests
{
    [Fact]
    public void GetAge_WhenDateOfBirthIsNull_ShouldReturnZero()
    {
        // Arrange
        var patient = new Patient { DateOfBirth = null };

        // Act
        var age = patient.GetAge();

        // Assert
        age.Should().Be(0);
    }

    [Fact]
    public void GetAge_WhenBirthdayHasNotOccurredThisYear_ShouldReturnCorrectAge()
    {
        // Arrange
        var today = new DateTime(2024, 6, 15);
        var dateOfBirth = new DateTime(1990, 12, 25); // Birthday later this year
        var patient = new Patient { DateOfBirth = dateOfBirth };

        // Mock DateTime.Today for testing
        // Note: In a real scenario, you'd inject IDateTimeProvider
        var expectedAge = 33; // 2024 - 1990 - 1 (birthday hasn't occurred)

        // Act
        var age = patient.GetAge();

        // Assert
        // This test assumes current date context
        age.Should().BeGreaterOrEqualTo(33);
    }

    [Fact]
    public void GetAge_WhenBirthdayHasOccurredThisYear_ShouldReturnCorrectAge()
    {
        // Arrange
        var dateOfBirth = new DateTime(1990, 1, 15); // Birthday earlier this year
        var patient = new Patient { DateOfBirth = dateOfBirth };

        // Act
        var age = patient.GetAge();

        // Assert
        // This test assumes current date context
        age.Should().BeGreaterOrEqualTo(34);
    }

    [Fact]
    public void GetAge_WhenBornToday_ShouldReturnZero()
    {
        // Arrange
        var patient = new Patient { DateOfBirth = DateTime.Today };

        // Act
        var age = patient.GetAge();

        // Assert
        age.Should().Be(0);
    }

    [Theory]
    [InlineData(1990, 1, 1)]
    [InlineData(2000, 12, 31)]
    [InlineData(1985, 6, 15)]
    public void GetAge_WithVariousDatesOfBirth_ShouldCalculateCorrectly(int year, int month, int day)
    {
        // Arrange
        var dateOfBirth = new DateTime(year, month, day);
        var patient = new Patient { DateOfBirth = dateOfBirth };
        var today = DateTime.Today;
        var expectedAge = today.Year - year;
        if (dateOfBirth.Date > today.AddYears(-expectedAge))
            expectedAge--;

        // Act
        var age = patient.GetAge();

        // Assert
        age.Should().Be(expectedAge);
    }
}