using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Patients.Commands;

public class UpdatePatientCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly Mock<IPhoneNumberValidationService> _phoneValidationServiceMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly Mock<ILogger<UpdatePatientCommandHandler>> _loggerMock;
    private readonly UpdatePatientCommandHandler _handler;

    public UpdatePatientCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _phoneValidationServiceMock = new Mock<IPhoneNumberValidationService>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _loggerMock = new Mock<ILogger<UpdatePatientCommandHandler>>();
        
        _unitOfWorkMock.Setup(x => x.Patients).Returns(_patientRepositoryMock.Object);
        
        _handler = new UpdatePatientCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _phoneValidationServiceMock.Object,
            _dateTimeProviderMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdatePatient()
    {
        // Arrange
        var clinicId = 1;
        var existingPatient = new Patient
        {
            Id = 1,
            ClinicId = clinicId,
            FullName = "John Doe",
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1990, 1, 1),
            PhoneNumbers = new List<PatientPhoneNumber>(),
            ChronicDiseases = new List<PatientChronicDisease>()
        };

        var command = new UpdatePatientCommand
        {
            Id = 1,
            FullName = "Jane Smith",
            Gender = Gender.Female,
            DateOfBirth = new DateTime(1985, 5, 15),
            PhoneNumbers = new List<UpdatePatientPhoneNumberDto>(),
            ChronicDiseaseIds = new List<int>()
        };

        _currentUserServiceMock.Setup(x => x.TryGetUserId(out It.Ref<int>.IsAny))
            .Returns((out int id) => { id = 1; return true; });
        _currentUserServiceMock.Setup(x => x.HasClinicAccess()).Returns(true);
        _currentUserServiceMock.Setup(x => x.TryGetClinicId(out It.Ref<int>.IsAny))
            .Returns((out int id) => { id = clinicId; return true; });
        _patientRepositoryMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPatient);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FullName.Should().Be("Jane Smith");
        result.Value.Gender.Should().Be(Gender.Female);
        
        existingPatient.FullName.Should().Be("Jane Smith");
        existingPatient.Gender.Should().Be(Gender.Female);
        existingPatient.DateOfBirth.Should().Be(new DateTime(1985, 5, 15));
        
        _patientRepositoryMock.Verify(x => x.UpdateAsync(existingPatient, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPatientNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdatePatientCommand { Id = 999 };
        
        _currentUserServiceMock.Setup(x => x.TryGetUserId(out It.Ref<int>.IsAny))
            .Returns((out int id) => { id = 1; return true; });
        _currentUserServiceMock.Setup(x => x.HasClinicAccess()).Returns(true);
        _currentUserServiceMock.Setup(x => x.TryGetClinicId(out It.Ref<int>.IsAny))
            .Returns((out int id) => { id = 1; return true; });
        _patientRepositoryMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(MessageCodes.Business.PATIENT_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoClinicAccess_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdatePatientCommand { Id = 1 };
        
        _currentUserServiceMock.Setup(x => x.TryGetUserId(out It.Ref<int>.IsAny))
            .Returns((out int id) => { id = 1; return true; });
        _currentUserServiceMock.Setup(x => x.HasClinicAccess()).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(MessageCodes.Authorization.USER_NO_CLINIC_ACCESS);
    }

    [Fact]
    public async Task Handle_WhenPatientBelongsToDifferentClinic_ShouldReturnFailure()
    {
        // Arrange
        var userClinicId = 1;
        var patientClinicId = 2;
        
        var existingPatient = new Patient
        {
            Id = 1,
            ClinicId = patientClinicId,
            FullName = "John Doe"
        };

        var command = new UpdatePatientCommand { Id = 1, FullName = "Updated Name" };

        _currentUserServiceMock.Setup(x => x.TryGetUserId(out It.Ref<int>.IsAny))
            .Returns((out int id) => { id = 1; return true; });
        _currentUserServiceMock.Setup(x => x.HasClinicAccess()).Returns(true);
        _currentUserServiceMock.Setup(x => x.TryGetClinicId(out It.Ref<int>.IsAny))
            .Returns((out int id) => { id = userClinicId; return true; });
        _patientRepositoryMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPatient);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(MessageCodes.Business.PATIENT_NOT_FOUND);
    }
}