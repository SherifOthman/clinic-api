using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Patients.Commands;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentAssertions;
using Moq;

namespace ClinicManagement.Application.Tests.Patients;

public class UpdatePatientHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IPhoneNormalizer> _phoneNormalizerMock = new();
    private readonly UpdatePatientCommandHandler _handler;

    public UpdatePatientHandlerTests()
    {
        _currentUserMock.Setup(x => x.CountryCode).Returns("EG");
        _phoneNormalizerMock.Setup(x => x.GetNationalNumber(It.IsAny<string>(), It.IsAny<string?>()))
            .Returns((string p, string? _) => p);
        _handler = new UpdatePatientCommandHandler(_uow, _currentUserMock.Object, _phoneNormalizerMock.Object);
    }

    private Patient MakePatient() => TestHandlerHelpers.CreateTestPatient();

    [Fact]
    public async Task Handle_ShouldFail_WhenPatientNotFound()
    {
        var result = await _handler.Handle(
            new UpdatePatientCommand(Guid.NewGuid(), "Full Name", "1990-01-01", "Male", null, null, null, null), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldUpdateFields_WhenPatientExists()
    {
        var patient = MakePatient();
        await _uow.Patients.AddAsync(patient);
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(
            new UpdatePatientCommand(patient.Id, "New Name", "1985-06-15", "Female", null, null, null, null), default);

        result.IsSuccess.Should().BeTrue();

        var updated = await _uow.Patients.GetByIdWithDetailsAsync(patient.Id);
        updated!.FullName.Should().Be("New Name");
        updated.Gender.Should().Be(Gender.Female);
    }

    [Fact]
    public async Task Handle_ShouldReplacePhoneNumbers_WhenProvided()
    {
        var patient = MakePatient();
        await _uow.Patients.AddAsync(patient);
        await _uow.SaveChangesAsync();

        _uow.Patients.AddPhone(new PatientPhone { PatientId = patient.Id, PhoneNumber = "+966500000000", NationalNumber = "500000000" });
        await _uow.SaveChangesAsync();

        await _handler.Handle(new UpdatePatientCommand(
            patient.Id, "Test Patient", "1990-01-01", "Male", null, null, null, null,
            ["+966500000001", "+966500000002"],
            null), default);

        var detail = await _uow.Patients.GetDetailAsync(patient.Id, false);
        detail!.Phones.Should().HaveCount(2);
        detail.Phones.Should().NotContain("+966500000000");
    }
}
