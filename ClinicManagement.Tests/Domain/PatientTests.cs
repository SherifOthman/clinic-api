using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Common.Exceptions;
using ClinicManagement.API.Entities;
using FluentAssertions;

namespace ClinicManagement.Tests.Domain;

public class PatientTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreatePatient()
    {
        // Arrange
        var patientCode = "PAT-2026-000001";
        var clinicId = Guid.NewGuid();
        var fullName = "John Doe";
        var dateOfBirth = new DateTime(1990, 1, 1);

        // Act
        var patient = Patient.Create(
            patientCode,
            clinicId,
            fullName,
            Gender.Male,
            dateOfBirth);

        // Assert
        patient.Should().NotBeNull();
        patient.PatientCode.Should().Be(patientCode);
        patient.FullName.Should().Be(fullName);
        patient.Gender.Should().Be(Gender.Male);
        patient.Age.Should().BeGreaterThan(30);
    }

    [Fact]
    public void Create_WithFutureDate_ShouldThrowException()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act
        var act = () => Patient.Create(
            "PAT-2026-000001",
            Guid.NewGuid(),
            "John Doe",
            Gender.Male,
            futureDate);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*future*");
    }

    [Fact]
    public void AddPhoneNumber_WithValidNumber_ShouldAddPhone()
    {
        // Arrange
        var patient = CreateValidPatient();
        var phoneNumber = "+1234567890";

        // Act
        patient.AddPhoneNumber(phoneNumber);

        // Assert
        patient.PhoneNumbers.Should().HaveCount(1);
        patient.PhoneNumbers.First().PhoneNumber.Should().Be(phoneNumber);
        patient.PhoneNumbers.First().IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void AddPhoneNumber_Duplicate_ShouldThrowException()
    {
        // Arrange
        var patient = CreateValidPatient();
        var phoneNumber = "+1234567890";
        patient.AddPhoneNumber(phoneNumber);

        // Act
        var act = () => patient.AddPhoneNumber(phoneNumber);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public void AddAllergy_WithValidData_ShouldAddAllergy()
    {
        // Arrange
        var patient = CreateValidPatient();
        var allergyName = "Penicillin";

        // Act
        patient.AddAllergy(allergyName, AllergySeverity.Severe, "Rash");

        // Assert
        patient.Allergies.Should().HaveCount(1);
        patient.Allergies.First().AllergyName.Should().Be(allergyName);
        patient.Allergies.First().Severity.Should().Be(AllergySeverity.Severe);
        patient.HasAllergy(allergyName).Should().BeTrue();
    }

    [Fact]
    public void AddAllergy_Duplicate_ShouldThrowException()
    {
        // Arrange
        var patient = CreateValidPatient();
        var allergyName = "Penicillin";
        patient.AddAllergy(allergyName, AllergySeverity.Severe);

        // Act
        var act = () => patient.AddAllergy(allergyName, AllergySeverity.Mild);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*already recorded*");
    }

    [Fact]
    public void GetCriticalAllergies_ShouldReturnOnlySevereAllergies()
    {
        // Arrange
        var patient = CreateValidPatient();
        patient.AddAllergy("Penicillin", AllergySeverity.Severe);
        patient.AddAllergy("Peanuts", AllergySeverity.LifeThreatening);
        patient.AddAllergy("Dust", AllergySeverity.Mild);

        // Act
        var criticalAllergies = patient.GetCriticalAllergies();

        // Assert
        criticalAllergies.Should().HaveCount(2);
        criticalAllergies.Should().OnlyContain(a => 
            a.Severity == AllergySeverity.Severe || 
            a.Severity == AllergySeverity.LifeThreatening);
    }

    [Fact]
    public void Age_ShouldCalculateCorrectly()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow.AddYears(-25);
        var patient = Patient.Create(
            "PAT-2026-000001",
            Guid.NewGuid(),
            "John Doe",
            Gender.Male,
            dateOfBirth);

        // Act & Assert
        patient.Age.Should().Be(25);
        patient.IsAdult.Should().BeTrue();
        patient.IsMinor.Should().BeFalse();
        patient.IsSenior.Should().BeFalse();
    }

    private static Patient CreateValidPatient()
    {
        return Patient.Create(
            "PAT-2026-000001",
            Guid.NewGuid(),
            "John Doe",
            Gender.Male,
            new DateTime(1990, 1, 1));
    }
}
