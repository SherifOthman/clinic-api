using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Patients.Commands.CreatePatient;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Patients.Commands;

public class CreatePatientCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly Mock<IPhoneNumberValidationService> _phoneValidationServiceMock;
    private readonly Mock<ILogger<CreatePatientCommandHandler>> _loggerMock;
    private readonly CreatePatientCommandHandler _handler;

    public CreatePatientCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _phoneValidationServiceMock = new Mock<IPhoneNumberValidationService>();
        _loggerMock = new Mock<ILogger<CreatePatientCommandHandler>>();

        _handler = new CreatePatientCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _dateTimeProviderMock.Object,
            _phoneValidationServiceMock.Object,
            _loggerMock.Object);

        // Setup default behavior
        _currentUserServiceMock.Setup(x => x.TryGetUserId(out It.Ref<int>.IsAny))
            .Callback(new TryGetUserIdCallback((out int userId) => { userId = 1; }))
            .Returns(true);
        _currentUserServiceMock.Setup(x => x.ClinicId).Returns(1);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(1);
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);
        
        // Setup phone validation service - handle two parameter calls
        _phoneValidationServiceMock.Setup(x => x.GetE164Format(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((phone, region) => phone.StartsWith("+") ? phone : $"+20{phone.TrimStart('0')}");
        _phoneValidationServiceMock.Setup(x => x.IsEgyptianPhoneNumber(It.IsAny<string>()))
            .Returns<string>(phone => phone.StartsWith("01") || phone.StartsWith("1") || phone.StartsWith("+20"));
    }

    // Helper delegate for TryGetUserId mock
    private delegate void TryGetUserIdCallback(out int userId);

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldCreatePatientSuccessfully()
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
                new() { PhoneNumber = "01098021214" }
            },
            ChronicDiseaseIds = new List<int> { 1, 2 }
        };

        var user = new User { Id = 1, ClinicId = 1 };
        _unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var capturedPatient = new Patient();
        _unitOfWorkMock.Setup(x => x.Patients.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()))
            .Callback<Patient, CancellationToken>((patient, ct) => 
            {
                capturedPatient = patient;
                patient.Id = 1; // Simulate database assignment
            })
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var createdPatient = new Patient
        {
            Id = 1,
            FullName = "John Michael Doe",
            ClinicId = 1,
            Address = "123 Main St",
            GeoNameId = 123456,
            PhoneNumbers = new List<PatientPhoneNumber>
            {
                new() { Id = 1, PhoneNumber = "+2001098021214" }
            }
        };

        _unitOfWorkMock.Setup(x => x.Patients.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPatient);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FullName.Should().Be("John Michael Doe");

        _unitOfWorkMock.Verify(x => x.Patients.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_WhenEgyptianPhoneNumber_ShouldFormatCorrectly()
    {
        // Arrange
        var command = new CreatePatientCommand
        {
            FullName = "Ahmed Ali Hassan",
            PhoneNumbers = new List<CreatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "01098021214" }
            }
        };

        var user = new User { Id = 1, ClinicId = 1 };
        _unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _phoneValidationServiceMock.Setup(x => x.GetE164Format("01098021214", It.IsAny<string>()))
            .Returns("+201098021214");

        var capturedPatient = new Patient();
        _unitOfWorkMock.Setup(x => x.Patients.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()))
            .Callback<Patient, CancellationToken>((patient, ct) => 
            {
                capturedPatient = patient;
                patient.Id = 1;
            })
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var createdPatient = new Patient
        {
            Id = 1,
            FullName = "Ahmed Ali Hassan",
            ClinicId = 1,
            PhoneNumbers = new List<PatientPhoneNumber>
            {
                new() { PhoneNumber = "+201098021214" }
            }
        };

        _unitOfWorkMock.Setup(x => x.Patients.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPatient);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        capturedPatient.PhoneNumbers.Should().HaveCount(1);
        capturedPatient.PhoneNumbers.First().PhoneNumber.Should().Be("+201098021214");
        
        _phoneValidationServiceMock.Verify(x => x.GetE164Format("01098021214", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenChronicDiseaseIds_ShouldCreateAssociations()
    {
        // Arrange
        var command = new CreatePatientCommand
        {
            FullName = "Sarah Mohamed Ibrahim",
            PhoneNumbers = new List<CreatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "+201098021214" }
            },
            ChronicDiseaseIds = new List<int> { 1, 2, 3 }
        };

        var user = new User { Id = 1, ClinicId = 1 };
        _unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var capturedPatient = new Patient();
        _unitOfWorkMock.Setup(x => x.Patients.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()))
            .Callback<Patient, CancellationToken>((patient, ct) => 
            {
                capturedPatient = patient;
                patient.Id = 1;
            })
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var createdPatient = new Patient
        {
            Id = 1,
            FullName = "Sarah Mohamed Ibrahim",
            ClinicId = 1,
            ChronicDiseases = new List<PatientChronicDisease>
            {
                new() { ChronicDiseaseId = 1 },
                new() { ChronicDiseaseId = 2 },
                new() { ChronicDiseaseId = 3 }
            }
        };

        _unitOfWorkMock.Setup(x => x.Patients.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPatient);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        capturedPatient.ChronicDiseases.Should().HaveCount(3);
        capturedPatient.ChronicDiseases.Select(cd => cd.ChronicDiseaseId).Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public async Task Handle_WhenLocationFields_ShouldSetCorrectly()
    {
        // Arrange
        var command = new CreatePatientCommand
        {
            FullName = "Omar Mahmoud Farouk",
            Address = "456 Elm Street, Cairo",
            GeoNameId = 360630, // Cairo, Egypt
            PhoneNumbers = new List<CreatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "+201098021214" }
            }
        };

        var user = new User { Id = 1, ClinicId = 1 };
        _unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var capturedPatient = new Patient();
        _unitOfWorkMock.Setup(x => x.Patients.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()))
            .Callback<Patient, CancellationToken>((patient, ct) => 
            {
                capturedPatient = patient;
                patient.Id = 1;
            })
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var createdPatient = new Patient
        {
            Id = 1,
            FullName = "Omar Mahmoud Farouk",
            ClinicId = 1,
            Address = "456 Elm Street, Cairo",
            GeoNameId = 360630
        };

        _unitOfWorkMock.Setup(x => x.Patients.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPatient);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        capturedPatient.Address.Should().Be("456 Elm Street, Cairo");
        capturedPatient.GeoNameId.Should().Be(360630);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnFailure()
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

        // Override the default setup to return false for this test
        _currentUserServiceMock.Setup(x => x.TryGetUserId(out It.Ref<int>.IsAny))
            .Callback(new TryGetUserIdCallback((out int userId) => { userId = 0; }))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Code.Should().NotBeNull();
        result.Code.Should().Be(MessageCodes.Authentication.USER_NOT_AUTHENTICATED);
    }
}