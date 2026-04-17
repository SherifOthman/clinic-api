using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.Patients.Commands;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentAssertions;

namespace ClinicManagement.Application.Tests.Patients;

public class RestorePatientHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly RestorePatientCommandHandler _handler;

    public RestorePatientHandlerTests()
    {
        _handler = new RestorePatientCommandHandler(_uow);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenPatientNotFound()
    {
        var result = await _handler.Handle(new RestorePatientCommand(Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenPatientNotDeleted()
    {
        var patient = TestHandlerHelpers.CreateTestPatient();
        await _uow.Patients.AddAsync(patient);
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new RestorePatientCommand(patient.Id), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldRestoreDeletedPatient()
    {
        var patient = TestHandlerHelpers.CreateTestPatient();
        patient.IsDeleted = true;
        await _uow.Patients.AddAsync(patient);
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new RestorePatientCommand(patient.Id), default);

        result.IsSuccess.Should().BeTrue();

        var restored = await _uow.Patients.GetDeletedByIdAsync(patient.Id);
        restored!.IsDeleted.Should().BeFalse();
    }
}
