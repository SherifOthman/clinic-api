using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Features.Patients.Commands;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Moq;

namespace ClinicManagement.Application.Tests.Patients;

public class RestorePatientHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IPatientRepository> _patientsMock = new();
    private readonly RestorePatientCommandHandler _handler;

    public RestorePatientHandlerTests()
    {
        _uowMock.Setup(u => u.Patients).Returns(_patientsMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        _handler = new RestorePatientCommandHandler(_uowMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenPatientNotFound()
    {
        var id = Guid.NewGuid();
        _patientsMock.Setup(x => x.GetDeletedByIdAsync(id, default)).ReturnsAsync((Patient?)null);

        var result = await _handler.Handle(new RestorePatientCommand(id), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenPatientNotDeleted()
    {
        var patient = TestHandlerHelpers.CreateTestPatient();
        patient.IsDeleted = false;
        _patientsMock.Setup(x => x.GetDeletedByIdAsync(patient.Id, default)).ReturnsAsync(patient);

        var result = await _handler.Handle(new RestorePatientCommand(patient.Id), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldRestoreDeletedPatient()
    {
        var patient = TestHandlerHelpers.CreateTestPatient();
        patient.IsDeleted = true;
        _patientsMock.Setup(x => x.GetDeletedByIdAsync(patient.Id, default)).ReturnsAsync(patient);

        var result = await _handler.Handle(new RestorePatientCommand(patient.Id), default);

        result.IsSuccess.Should().BeTrue();
        patient.IsDeleted.Should().BeFalse();
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
