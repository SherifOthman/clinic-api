using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Entities;
using FluentAssertions;

namespace ClinicManagement.Domain.Tests.Entities;

public class PatientTests
{
    [Fact]
    public void Age_ShouldCalculateCorrectly_WhenBirthdayHasPassed()
    {
        // Arrange
        var today = new DateTime(2026, 2, 10);
        var birthDate = new DateTime(1990, 1, 15); // Birthday already passed this year
        
        var patient = new Patient
        {
            DateOfBirth = birthDate
        };

        // Act
        var age = patient.Age;

        // Assert
        age.Should().Be(36);
    }

    [Fact]
    public void Age_ShouldCalculateCorrectly_WhenBirthdayHasNotPassed()
    {
        // Arrange
        var today = new DateTime(2026, 2, 10);
        var birthDate = new DateTime(1990, 3, 15); // Birthday hasn't passed yet this year
        
        var patient = new Patient
        {
            DateOfBirth = birthDate
        };

        // Act
        var age = patient.Age;

        // Assert
        age.Should().Be(35); // Still 35 until March 15
    }

    [Fact]
    public void IsAdult_ShouldReturnTrue_WhenAgeIs18OrMore()
    {
        // Arrange
        var patient = new Patient
        {
            DateOfBirth = DateTime.UtcNow.AddYears(-18)
        };

        // Act & Assert
        patient.IsAdult.Should().BeTrue();
    }

    [Fact]
    public void IsAdult_ShouldReturnFalse_WhenAgeIsLessThan18()
    {
        // Arrange
        var patient = new Patient
        {
            DateOfBirth = DateTime.UtcNow.AddYears(-17)
        };

        // Act & Assert
        patient.IsAdult.Should().BeFalse();
    }

    [Fact]
    public void IsMinor_ShouldReturnTrue_WhenAgeIsLessThan18()
    {
        // Arrange
        var patient = new Patient
        {
            DateOfBirth = DateTime.UtcNow.AddYears(-10)
        };

        // Act & Assert
        patient.IsMinor.Should().BeTrue();
    }

    [Fact]
    public void IsMinor_ShouldReturnFalse_WhenAgeIs18OrMore()
    {
        // Arrange
        var patient = new Patient
        {
            DateOfBirth = DateTime.UtcNow.AddYears(-25)
        };

        // Act & Assert
        patient.IsMinor.Should().BeFalse();
    }

    [Fact]
    public void IsSenior_ShouldReturnTrue_WhenAgeIs65OrMore()
    {
        // Arrange
        var patient = new Patient
        {
            DateOfBirth = DateTime.UtcNow.AddYears(-70)
        };

        // Act & Assert
        patient.IsSenior.Should().BeTrue();
    }

    [Fact]
    public void IsSenior_ShouldReturnFalse_WhenAgeIsLessThan65()
    {
        // Arrange
        var patient = new Patient
        {
            DateOfBirth = DateTime.UtcNow.AddYears(-50)
        };

        // Act & Assert
        patient.IsSenior.Should().BeFalse();
    }

    [Fact]
    public void PrimaryPhoneNumber_ShouldReturnFirstPhoneNumber_WhenPhoneNumbersExist()
    {
        // Arrange
        var patient = new Patient
        {
            PhoneNumbers = new List<PatientPhone>
            {
                new() { PhoneNumber = "+1234567890" },
                new() { PhoneNumber = "+0987654321" }
            }
        };

        // Act
        var primaryPhone = patient.PrimaryPhoneNumber;

        // Assert
        primaryPhone.Should().Be("+1234567890");
    }

    [Fact]
    public void PrimaryPhoneNumber_ShouldReturnEmptyString_WhenNoPhoneNumbers()
    {
        // Arrange
        var patient = new Patient
        {
            PhoneNumbers = new List<PatientPhone>()
        };

        // Act
        var primaryPhone = patient.PrimaryPhoneNumber;

        // Assert
        primaryPhone.Should().BeEmpty();
    }

    [Fact]
    public void HasChronicDiseases_ShouldReturnTrue_WhenPatientHasChronicDiseases()
    {
        // Arrange
        var patient = new Patient
        {
            ChronicDiseases = new List<PatientChronicDisease>
            {
                new() { ChronicDiseaseId = Guid.NewGuid() }
            }
        };

        // Act & Assert
        patient.HasChronicDiseases.Should().BeTrue();
    }

    [Fact]
    public void HasChronicDiseases_ShouldReturnFalse_WhenPatientHasNoChronicDiseases()
    {
        // Arrange
        var patient = new Patient
        {
            ChronicDiseases = new List<PatientChronicDisease>()
        };

        // Act & Assert
        patient.HasChronicDiseases.Should().BeFalse();
    }

    [Fact]
    public void ChronicDiseaseCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var patient = new Patient
        {
            ChronicDiseases = new List<PatientChronicDisease>
            {
                new() { ChronicDiseaseId = Guid.NewGuid() },
                new() { ChronicDiseaseId = Guid.NewGuid() },
                new() { ChronicDiseaseId = Guid.NewGuid() }
            }
        };

        // Act
        var count = patient.ChronicDiseaseCount;

        // Assert
        count.Should().Be(3);
    }

    [Fact]
    public void ChronicDiseaseCount_ShouldReturnZero_WhenNoChronicDiseases()
    {
        // Arrange
        var patient = new Patient
        {
            ChronicDiseases = new List<PatientChronicDisease>()
        };

        // Act
        var count = patient.ChronicDiseaseCount;

        // Assert
        count.Should().Be(0);
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    public void Gender_ShouldBeSettable(Gender gender)
    {
        // Arrange
        var patient = new Patient();

        // Act
        patient.Gender = gender;

        // Assert
        patient.Gender.Should().Be(gender);
    }

    [Fact]
    public void Patient_ShouldInitializeCollections()
    {
        // Arrange & Act
        var patient = new Patient();

        // Assert
        patient.PhoneNumbers.Should().NotBeNull().And.BeEmpty();
        patient.ChronicDiseases.Should().NotBeNull().And.BeEmpty();
        patient.MedicalFiles.Should().NotBeNull().And.BeEmpty();
        patient.Invoices.Should().NotBeNull().And.BeEmpty();
    }
}
