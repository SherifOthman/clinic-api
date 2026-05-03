using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
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
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IPatientRepository> _patientsMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IPhoneNormalizer> _phoneNormalizerMock = new();
    private readonly UpdatePatientCommandHandler _handler;

    public UpdatePatientHandlerTests()
    {
        _uowMock.Setup(u => u.Patients).Returns(_patientsMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        _currentUserMock.Setup(x => x.CountryCode).Returns("EG");
        _phoneNormalizerMock.Setup(x => x.GetNationalNumber(It.IsAny<string>(), It.IsAny<string?>()))
            .Returns((string p, string? _) => p);

        _handler = new UpdatePatientCommandHandler(_uowMock.Object, _currentUserMock.Object, _phoneNormalizerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenPatientNotFound()
    {
        var id = Guid.NewGuid();
        _patientsMock.Setup(x => x.GetByIdWithDetailsAsync(id, default)).ReturnsAsync((Patient?)null);

        var result = await _handler.Handle(
            new UpdatePatientCommand(id, "Full Name", "1990-01-01", "Male", null, null, null, null), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldUpdateFields_WhenPatientExists()
    {
        var patient = TestHandlerHelpers.CreateTestPatient();
        _patientsMock.Setup(x => x.GetByIdWithDetailsAsync(patient.Id, default)).ReturnsAsync(patient);

        var result = await _handler.Handle(
            new UpdatePatientCommand(patient.Id, "New Name", "1985-06-15", "Female", null, null, null, null), default);

        result.IsSuccess.Should().BeTrue();
        patient.FullName.Should().Be("New Name");
        patient.Gender.Should().Be(Gender.Female);
        patient.DateOfBirth.Should().Be(new DateOnly(1985, 6, 15));
    }

    [Fact]
    public async Task Handle_ShouldReplacePhoneNumbers_WhenProvided()
    {
        var patient = TestHandlerHelpers.CreateTestPatient();
        // Seed an existing phone on the in-memory object
        patient.Phones.Add(new PatientPhone { PatientId = patient.Id, PhoneNumber = "+966500000000", NationalNumber = "500000000" });
        _patientsMock.Setup(x => x.GetByIdWithDetailsAsync(patient.Id, default)).ReturnsAsync(patient);

        var result = await _handler.Handle(new UpdatePatientCommand(
            patient.Id, "Test Patient", "1990-01-01", "Male", null, null, null, null,
            ["+966500000001", "+966500000002"],
            null), default);

        result.IsSuccess.Should().BeTrue();
        // RemovePhone called for old, AddPhone called for new ones
        _patientsMock.Verify(x => x.RemovePhone(It.IsAny<PatientPhone>()), Times.Once);
        _patientsMock.Verify(x => x.AddPhone(It.IsAny<PatientPhone>()), Times.Exactly(2));
    }
}
