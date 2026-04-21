using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentAssertions;

namespace ClinicManagement.Application.Tests.Patients;

public class GetPatientDetailHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly GetPatientDetailHandler _handler;

    public GetPatientDetailHandlerTests()
    {
        _handler = new GetPatientDetailHandler(_uow);
    }

    private Patient MakePatient(Gender gender = Gender.Male)
    {
        var person = new Person { FirstName = "Test", LastName = "Patient", Gender = gender, DateOfBirth = new DateOnly(1990, 6, 15) };
        return new Patient
        {
            ClinicId = Guid.NewGuid(), PatientCode = "0001",
            PersonId = person.Id, Person = person, CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenPatientNotFound()
    {
        var result = await _handler.Handle(new GetPatientDetailQuery(Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnDetail_WithCorrectFields()
    {
        var patient = MakePatient(Gender.Female);
        await _uow.Patients.AddAsync(patient);
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new GetPatientDetailQuery(patient.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.FullName.Should().Be("Test Patient");
        result.Value.Gender.Should().Be("Female");
        result.Value.PatientCode.Should().Be("0001");
    }

    [Fact]
    public async Task Handle_ShouldIncludePhoneNumbers()
    {
        var patient = MakePatient();
        await _uow.Patients.AddAsync(patient);
        await _uow.SaveChangesAsync();

        _uow.Patients.AddPhone(new PatientPhone { PatientId = patient.Id, PhoneNumber = "+966500000001", NationalNumber = "500000001" });
        _uow.Patients.AddPhone(new PatientPhone { PatientId = patient.Id, PhoneNumber = "+966500000002", NationalNumber = "500000002" });
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new GetPatientDetailQuery(patient.Id), default);

        result.Value.PhoneNumbers.Should().HaveCount(2);
        result.Value.PhoneNumbers.Should().Contain("+966500000001");
    }

    [Fact]
    public async Task Handle_ShouldIncludeChronicDiseases()
    {
        var patient = MakePatient();
        await _uow.Patients.AddAsync(patient);
        var disease = new ChronicDisease { NameEn = "Diabetes", NameAr = "السكري" };
        await _uow.ChronicDiseases.AddAsync(disease);
        await _uow.SaveChangesAsync();

        _uow.Patients.AddChronicDisease(new PatientChronicDisease
            { PatientId = patient.Id, ChronicDiseaseId = disease.Id });
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new GetPatientDetailQuery(patient.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.ChronicDiseases.Should().HaveCount(1);
        result.Value.ChronicDiseases[0].NameEn.Should().Be("Diabetes");
    }
}
