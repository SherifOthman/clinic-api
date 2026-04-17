using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.Patients.Commands;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentAssertions;

namespace ClinicManagement.Application.Tests.Patients;

public class DeletePatientHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly DeletePatientCommandHandler _handler;

    public DeletePatientHandlerTests()
    {
        _handler = new DeletePatientCommandHandler(_uow);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenPatientNotFound()
    {
        var result = await _handler.Handle(new DeletePatientCommand(Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldSoftDelete_WhenPatientExists()
    {
        var patient = TestHandlerHelpers.CreateTestPatient(patientCode: "00000001");
        await _uow.Patients.AddAsync(patient);
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new DeletePatientCommand(patient.Id), default);

        result.IsSuccess.Should().BeTrue();

        // GetDeletedByIdAsync bypasses the filter — confirms soft-delete was applied
        var deleted = await _uow.Patients.GetDeletedByIdAsync(patient.Id);
        deleted!.IsDeleted.Should().BeTrue();
    }
}
