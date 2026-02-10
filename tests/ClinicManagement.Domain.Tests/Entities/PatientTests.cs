using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Exceptions;
using ClinicManagement.Domain.Entities;
using FluentAssertions;

namespace ClinicManagement.Domain.Tests.Entities;

public class PatientTests
{
    private static Patient CreateTestPatient(DateTime dateOfBirth)
    {
        return Patient.Create(
            "P-001",
            Guid.NewGuid(),
            "Test Patient",
            Gender.Male,
            dateOfBirth
        );
    }

    #region Creation Tests

    [Fact]
    public void Create_ShouldCreatePatientWithCorrectProperties()
    {
        // Arrange
        var patientCode = "P-001";
        var clinicId = Guid.NewGuid();
        var fullName = "John Doe";
        var gender = Gender.Male;
        var dateOfBirth = new DateTime(1990, 1, 15);
        var cityGeoNameId = 12345;

        // Act
        var patient = Patient.Create(patientCode, clinicId, fullName, gender, dateOfBirth, cityGeoNameId);

        // Assert
        patient.Should().NotBeNull();
        patient.PatientCode.Should().Be(patientCode);
        patient.ClinicId.Should().Be(clinicId);
        patient.FullName.Should().Be(fullName);
        patient.Gender.Should().Be(gender);
        patient.DateOfBirth.Should().Be(dateOfBirth);
        patient.CityGeoNameId.Should().Be(cityGeoNameId);
        patient.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Create_ShouldRaisePatientRegisteredEvent()
    {
        // Arrange
        var patientCode = "P-001";
        var clinicId = Guid.NewGuid();
        var fullName = "John Doe";
        var gender = Gender.Male;
        var dateOfBirth = new DateTime(1990, 1, 15);

        // Act
        var patient = Patient.Create(patientCode, clinicId, fullName, gender, dateOfBirth);

        // Assert
        patient.DomainEvents.Should().HaveCount(1);
        var domainEvent = patient.DomainEvents.First();
        domainEvent.Should().BeOfType<ClinicManagement.Domain.Events.PatientRegisteredEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_ShouldThrowException_WhenPatientCodeIsInvalid(string? patientCode)
    {
        // Arrange
        var clinicId = Guid.NewGuid();
        var fullName = "John Doe";
        var gender = Gender.Male;
        var dateOfBirth = new DateTime(1990, 1, 15);

        // Act
        var act = () => Patient.Create(patientCode!, clinicId, fullName, gender, dateOfBirth);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Patient code is required*");
    }

    [Fact]
    public void Create_ShouldThrowException_WhenClinicIdIsEmpty()
    {
        // Arrange
        var patientCode = "P-001";
        var clinicId = Guid.Empty;
        var fullName = "John Doe";
        var gender = Gender.Male;
        var dateOfBirth = new DateTime(1990, 1, 15);

        // Act
        var act = () => Patient.Create(patientCode, clinicId, fullName, gender, dateOfBirth);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Clinic ID is required*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_ShouldThrowException_WhenFullNameIsInvalid(string? fullName)
    {
        // Arrange
        var patientCode = "P-001";
        var clinicId = Guid.NewGuid();
        var gender = Gender.Male;
        var dateOfBirth = new DateTime(1990, 1, 15);

        // Act
        var act = () => Patient.Create(patientCode, clinicId, fullName!, gender, dateOfBirth);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Full name is required*");
    }

    [Fact]
    public void Create_ShouldThrowException_WhenDateOfBirthIsInFuture()
    {
        // Arrange
        var patientCode = "P-001";
        var clinicId = Guid.NewGuid();
        var fullName = "John Doe";
        var gender = Gender.Male;
        var dateOfBirth = DateTime.UtcNow.AddDays(1);

        // Act
        var act = () => Patient.Create(patientCode, clinicId, fullName, gender, dateOfBirth);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Date of birth cannot be in the future*");
    }

    [Fact]
    public void Create_ShouldThrowException_WhenDateOfBirthIsTooOld()
    {
        // Arrange
        var patientCode = "P-001";
        var clinicId = Guid.NewGuid();
        var fullName = "John Doe";
        var gender = Gender.Male;
        var dateOfBirth = DateTime.UtcNow.AddYears(-151);

        // Act
        var act = () => Patient.Create(patientCode, clinicId, fullName, gender, dateOfBirth);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Date of birth is too far in the past*");
    }

    #endregion

    #region UpdateInfo Tests

    [Fact]
    public void UpdateInfo_ShouldUpdatePatientProperties()
    {
        // Arrange
        var patient = CreateTestPatient(new DateTime(1990, 1, 15));
        var newFullName = "Jane Smith";
        var newGender = Gender.Female;
        var newDateOfBirth = new DateTime(1995, 5, 20);
        var newCityGeoNameId = 54321;

        // Act
        patient.UpdateInfo(newFullName, newGender, newDateOfBirth, newCityGeoNameId);

        // Assert
        patient.FullName.Should().Be(newFullName);
        patient.Gender.Should().Be(newGender);
        patient.DateOfBirth.Should().Be(newDateOfBirth);
        patient.CityGeoNameId.Should().Be(newCityGeoNameId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UpdateInfo_ShouldThrowException_WhenFullNameIsInvalid(string? fullName)
    {
        // Arrange
        var patient = CreateTestPatient(new DateTime(1990, 1, 15));
        var gender = Gender.Female;
        var dateOfBirth = new DateTime(1995, 5, 20);

        // Act
        var act = () => patient.UpdateInfo(fullName!, gender, dateOfBirth, null);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Full name is required*");
    }

    [Fact]
    public void UpdateInfo_ShouldThrowException_WhenDateOfBirthIsInFuture()
    {
        // Arrange
        var patient = CreateTestPatient(new DateTime(1990, 1, 15));
        var fullName = "Jane Smith";
        var gender = Gender.Female;
        var dateOfBirth = DateTime.UtcNow.AddDays(1);

        // Act
        var act = () => patient.UpdateInfo(fullName, gender, dateOfBirth, null);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Date of birth cannot be in the future*");
    }

    #endregion

    #region Phone Number Tests

    [Fact]
    public void AddPhoneNumber_ShouldAddPhoneToCollection()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));
        var phoneNumber = "+1234567890";

        // Act
        patient.AddPhoneNumber(phoneNumber, isPrimary: true);

        // Assert
        patient.PhoneNumbers.Should().HaveCount(1);
        patient.PhoneNumbers.First().PhoneNumber.Should().Be(phoneNumber);
        patient.PhoneNumbers.First().IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void AddPhoneNumber_ShouldSetFirstPhoneAsPrimary_WhenNoPhonesExist()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));
        var phoneNumber = "+1234567890";

        // Act
        patient.AddPhoneNumber(phoneNumber, isPrimary: false);

        // Assert
        patient.PhoneNumbers.Should().HaveCount(1);
        patient.PhoneNumbers.First().IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void AddPhoneNumber_ShouldUnsetOtherPrimaryPhones_WhenAddingNewPrimary()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));
        patient.AddPhoneNumber("+1234567890", isPrimary: true);

        // Act
        patient.AddPhoneNumber("+0987654321", isPrimary: true);

        // Assert
        patient.PhoneNumbers.Should().HaveCount(2);
        patient.PhoneNumbers.Count(p => p.IsPrimary).Should().Be(1);
        patient.PhoneNumbers.First(p => p.PhoneNumber == "+0987654321").IsPrimary.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void AddPhoneNumber_ShouldThrowException_WhenPhoneNumberIsInvalid(string? phoneNumber)
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));

        // Act
        var act = () => patient.AddPhoneNumber(phoneNumber!);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Phone number is required*");
    }

    [Fact]
    public void AddPhoneNumber_ShouldThrowException_WhenPhoneNumberAlreadyExists()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));
        var phoneNumber = "+1234567890";
        patient.AddPhoneNumber(phoneNumber);

        // Act
        var act = () => patient.AddPhoneNumber(phoneNumber);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public void RemovePhoneNumber_ShouldRemovePhoneFromCollection()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));
        patient.AddPhoneNumber("+1234567890");
        patient.AddPhoneNumber("+0987654321");

        // Act
        patient.RemovePhoneNumber("+1234567890");

        // Assert
        patient.PhoneNumbers.Should().HaveCount(1);
        patient.PhoneNumbers.First().PhoneNumber.Should().Be("+0987654321");
    }

    [Fact]
    public void RemovePhoneNumber_ShouldThrowException_WhenPhoneNotFound()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));
        patient.AddPhoneNumber("+1234567890");

        // Act
        var act = () => patient.RemovePhoneNumber("+0987654321");

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public void RemovePhoneNumber_ShouldThrowException_WhenRemovingLastPhone()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));
        patient.AddPhoneNumber("+1234567890");

        // Act
        var act = () => patient.RemovePhoneNumber("+1234567890");

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Cannot remove the last phone number*");
    }

    [Fact]
    public void RemovePhoneNumber_ShouldSetNewPrimary_WhenRemovingPrimaryPhone()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));
        patient.AddPhoneNumber("+1234567890", isPrimary: true);
        patient.AddPhoneNumber("+0987654321");

        // Act
        patient.RemovePhoneNumber("+1234567890");

        // Assert
        patient.PhoneNumbers.Should().HaveCount(1);
        patient.PhoneNumbers.First().IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void SetPrimaryPhoneNumber_ShouldSetPhoneAsPrimary()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));
        patient.AddPhoneNumber("+1234567890");
        patient.AddPhoneNumber("+0987654321");

        // Act
        patient.SetPrimaryPhoneNumber("+0987654321");

        // Assert
        patient.PhoneNumbers.First(p => p.PhoneNumber == "+0987654321").IsPrimary.Should().BeTrue();
        patient.PhoneNumbers.First(p => p.PhoneNumber == "+1234567890").IsPrimary.Should().BeFalse();
    }

    [Fact]
    public void SetPrimaryPhoneNumber_ShouldThrowException_WhenPhoneNotFound()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));
        patient.AddPhoneNumber("+1234567890");

        // Act
        var act = () => patient.SetPrimaryPhoneNumber("+0987654321");

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*not found*");
    }

    #endregion

    #region Chronic Disease Tests

    [Fact]
    public void AddChronicDisease_ShouldAddDiseaseToCollection()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));
        var diseaseId = Guid.NewGuid();

        // Act
        patient.AddChronicDisease(diseaseId);

        // Assert
        patient.ChronicDiseases.Should().HaveCount(1);
        patient.ChronicDiseases.First().ChronicDiseaseId.Should().Be(diseaseId);
    }

    [Fact]
    public void AddChronicDisease_ShouldThrowException_WhenDiseaseIdIsEmpty()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));

        // Act
        var act = () => patient.AddChronicDisease(Guid.Empty);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*Chronic disease ID is required*");
    }

    [Fact]
    public void AddChronicDisease_ShouldThrowException_WhenDiseaseAlreadyExists()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));
        var diseaseId = Guid.NewGuid();
        patient.AddChronicDisease(diseaseId);

        // Act
        var act = () => patient.AddChronicDisease(diseaseId);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*already associated*");
    }

    [Fact]
    public void RemoveChronicDisease_ShouldRemoveDiseaseFromCollection()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));
        var diseaseId1 = Guid.NewGuid();
        var diseaseId2 = Guid.NewGuid();
        patient.AddChronicDisease(diseaseId1);
        patient.AddChronicDisease(diseaseId2);

        // Act
        patient.RemoveChronicDisease(diseaseId1);

        // Assert
        patient.ChronicDiseases.Should().HaveCount(1);
        patient.ChronicDiseases.First().ChronicDiseaseId.Should().Be(diseaseId2);
    }

    [Fact]
    public void RemoveChronicDisease_ShouldThrowException_WhenDiseaseNotFound()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));
        var diseaseId = Guid.NewGuid();

        // Act
        var act = () => patient.RemoveChronicDisease(diseaseId);

        // Assert
        act.Should().Throw<InvalidBusinessOperationException>()
            .WithMessage("*not found*");
    }

    #endregion

    #region Calculated Properties Tests

    [Fact]
    public void Age_ShouldCalculateCorrectly_WhenBirthdayHasPassed()
    {
        // Arrange
        var today = new DateTime(2026, 2, 10);
        var birthDate = new DateTime(1990, 1, 15); // Birthday already passed this year
        
        var patient = CreateTestPatient(birthDate);

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
        
        var patient = CreateTestPatient(birthDate);

        // Act
        var age = patient.Age;

        // Assert
        age.Should().Be(35); // Still 35 until March 15
    }

    [Fact]
    public void IsAdult_ShouldReturnTrue_WhenAgeIs18OrMore()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-18));

        // Act & Assert
        patient.IsAdult.Should().BeTrue();
    }

    [Fact]
    public void IsAdult_ShouldReturnFalse_WhenAgeIsLessThan18()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-17));

        // Act & Assert
        patient.IsAdult.Should().BeFalse();
    }

    [Fact]
    public void IsMinor_ShouldReturnTrue_WhenAgeIsLessThan18()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-10));

        // Act & Assert
        patient.IsMinor.Should().BeTrue();
    }

    [Fact]
    public void IsMinor_ShouldReturnFalse_WhenAgeIs18OrMore()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-25));

        // Act & Assert
        patient.IsMinor.Should().BeFalse();
    }

    [Fact]
    public void IsSenior_ShouldReturnTrue_WhenAgeIs65OrMore()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-70));

        // Act & Assert
        patient.IsSenior.Should().BeTrue();
    }

    [Fact]
    public void IsSenior_ShouldReturnFalse_WhenAgeIsLessThan65()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-50));

        // Act & Assert
        patient.IsSenior.Should().BeFalse();
    }

    [Fact]
    public void PrimaryPhoneNumber_ShouldReturnFirstPhoneNumber_WhenPhoneNumbersExist()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));
        patient.AddPhoneNumber("+1234567890");
        patient.AddPhoneNumber("+0987654321");

        // Act
        var primaryPhone = patient.PrimaryPhoneNumber;

        // Assert
        primaryPhone.Should().Be("+1234567890");
    }

    [Fact]
    public void PrimaryPhoneNumber_ShouldReturnEmptyString_WhenNoPhoneNumbers()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));

        // Act
        var primaryPhone = patient.PrimaryPhoneNumber;

        // Assert
        primaryPhone.Should().BeEmpty();
    }

    [Fact]
    public void HasChronicDiseases_ShouldReturnTrue_WhenPatientHasChronicDiseases()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));
        patient.AddChronicDisease(Guid.NewGuid());

        // Act & Assert
        patient.HasChronicDiseases.Should().BeTrue();
    }

    [Fact]
    public void HasChronicDiseases_ShouldReturnFalse_WhenPatientHasNoChronicDiseases()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));

        // Act & Assert
        patient.HasChronicDiseases.Should().BeFalse();
    }

    [Fact]
    public void ChronicDiseaseCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));
        patient.AddChronicDisease(Guid.NewGuid());
        patient.AddChronicDisease(Guid.NewGuid());
        patient.AddChronicDisease(Guid.NewGuid());

        // Act
        var count = patient.ChronicDiseaseCount;

        // Assert
        count.Should().Be(3);
    }

    [Fact]
    public void ChronicDiseaseCount_ShouldReturnZero_WhenNoChronicDiseases()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));

        // Act
        var count = patient.ChronicDiseaseCount;

        // Assert
        count.Should().Be(0);
    }

    #endregion

    #region Encapsulation Tests

    [Fact]
    public void Patient_ShouldInitializeCollections()
    {
        // Arrange & Act
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));

        // Assert
        patient.PhoneNumbers.Should().NotBeNull();
        patient.ChronicDiseases.Should().NotBeNull();
        patient.MedicalFiles.Should().NotBeNull();
        patient.Invoices.Should().NotBeNull();
    }

    [Fact]
    public void PhoneNumbers_ShouldBeReadOnly()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));

        // Act & Assert
        patient.PhoneNumbers.Should().BeAssignableTo<IReadOnlyCollection<PatientPhone>>();
    }

    [Fact]
    public void ChronicDiseases_ShouldBeReadOnly()
    {
        // Arrange
        var patient = CreateTestPatient(DateTime.UtcNow.AddYears(-30));

        // Act & Assert
        patient.ChronicDiseases.Should().BeAssignableTo<IReadOnlyCollection<PatientChronicDisease>>();
    }

    #endregion
}
