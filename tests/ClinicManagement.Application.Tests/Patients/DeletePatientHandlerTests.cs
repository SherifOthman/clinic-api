using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Features.Patients.Commands;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Moq;

namespace ClinicManagement.Application.Tests.Patients;

public class DeletePatientHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IPatientRepository> _patientsMock = new();
    private readonly DeletePatientCommandHandler _handler;

    public DeletePatientHandlerTests()
    {
        _uowMock.Setup(u => u.Patients).Returns(_patientsMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        _handler = new DeletePatientCommandHandler(_uowMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenPatientNotFound()
    {
        var id = Guid.NewGuid();
        _patientsMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync((Patient?)null);

        var result = await _handler.Handle(new DeletePatientCommand(id), default);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCodes.PATIENT_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_ShouldSoftDelete_WhenPatientExists()
    {
        var patient = TestHandlerHelpers.CreateTestPatient(patientCode: "0001");
        _patientsMock.Setup(x => x.GetByIdAsync(patient.Id, default)).ReturnsAsync(patient);

        var result = await _handler.Handle(new DeletePatientCommand(patient.Id), default);

        result.IsSuccess.Should().BeTrue();
        // SoftDelete() sets IsDeleted = true on the in-memory object
        patient.IsDeleted.Should().BeTrue();
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
