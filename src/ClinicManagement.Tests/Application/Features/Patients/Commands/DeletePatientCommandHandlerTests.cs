using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Patients.Commands.DeletePatient;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Patients.Commands;

public class DeletePatientCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly Mock<ILogger<DeletePatientCommandHandler>> _loggerMock;
    private readonly DeletePatientCommandHandler _handler;

    public DeletePatientCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _loggerMock = new Mock<ILogger<DeletePatientCommandHandler>>();
        
        _unitOfWorkMock.Setup(x => x.Patients).Returns(_patientRepositoryMock.Object);
        
        _handler = new DeletePatientCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeletePatient()
    {
        // Arrange
        var clinicId = 1;
        var existingPatient = new Patient
        {
            Id = 1,
            ClinicId = clinicId,
            FullName = "John Doe"
        };

        var command = new DeletePatientCommand { Id = 1 };

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
        
        _patientRepositoryMock.Verify(x => x.UpdateAsync(existingPatient, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPatientNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeletePatientCommand { Id = 999 };
        
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
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeletePatientCommand { Id = 1 };
        
        _currentUserServiceMock.Setup(x => x.TryGetUserId(out It.Ref<int>.IsAny))
            .Returns((out int id) => { id = 0; return false; });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(MessageCodes.Authentication.USER_NOT_AUTHENTICATED);
    }
}