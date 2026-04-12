using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentAssertions;
namespace ClinicManagement.Application.Tests.Patients;

public class GetPatientsQueryHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly GetPatientsQueryHandler _handler;

    public GetPatientsQueryHandlerTests()
    {
        _handler = new GetPatientsQueryHandler(_uow);
    }

    private Patient MakePatient(string name, Gender gender, string code, int chronicDiseaseCount = 0)
    {
        var patient = new Patient
        {
            ClinicId = Guid.NewGuid(), PatientCode = code, FullName = name,
            DateOfBirth = new DateOnly(1990, 1, 1), Gender = gender, CreatedAt = DateTimeOffset.UtcNow,
        };
        for (var i = 0; i < chronicDiseaseCount; i++)
            patient.ChronicDiseases.Add(new PatientChronicDisease { ChronicDiseaseId = Guid.NewGuid() });
        return patient;
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResults()
    {
        await _uow.Patients.AddAsync(MakePatient("Alice",   Gender.Female, "0000001"));
        await _uow.Patients.AddAsync(MakePatient("Bob",     Gender.Male,   "0000002"));
        await _uow.Patients.AddAsync(MakePatient("Charlie", Gender.Male,   "0000003"));
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new GetPatientsQuery(null, 1, 2, null, "asc", null), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(3);
        result.Value.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldFilterBySearchTerm()
    {
        await _uow.Patients.AddAsync(MakePatient("Ahmed Ali",    Gender.Male,   "0000001"));
        await _uow.Patients.AddAsync(MakePatient("Sara Mohamed", Gender.Female, "0000002"));
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new GetPatientsQuery("Ahmed", 1, 10, null, "asc", null), default);

        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.Single().FullName.Should().Be("Ahmed Ali");
    }

    [Fact]
    public async Task Handle_ShouldFilterByGender()
    {
        await _uow.Patients.AddAsync(MakePatient("Male Patient",   Gender.Male,   "0000001"));
        await _uow.Patients.AddAsync(MakePatient("Female Patient", Gender.Female, "0000002"));
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new GetPatientsQuery(null, 1, 10, null, "asc", Gender: "Female"), default);

        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.Single().Gender.Should().Be("Female");
    }

    [Fact]
    public async Task Handle_ShouldSortByName_Ascending()
    {
        await _uow.Patients.AddAsync(MakePatient("Zara",    Gender.Male, "0000001"));
        await _uow.Patients.AddAsync(MakePatient("Ahmed",   Gender.Male, "0000002"));
        await _uow.Patients.AddAsync(MakePatient("Mohamed", Gender.Male, "0000003"));
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new GetPatientsQuery(null, 1, 10, "fullName", "asc", null), default);

        result.Value.Items.Select(p => p.FullName).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Handle_ShouldSortByChronicDiseaseCount_Ascending()
    {
        await _uow.Patients.AddAsync(MakePatient("Three Diseases", Gender.Male,   "0000001", chronicDiseaseCount: 3));
        await _uow.Patients.AddAsync(MakePatient("No Diseases",    Gender.Female, "0000002", chronicDiseaseCount: 0));
        await _uow.Patients.AddAsync(MakePatient("One Disease",    Gender.Male,   "0000003", chronicDiseaseCount: 1));
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(
            new GetPatientsQuery(null, 1, 10, "chronicDiseaseCount", "asc", null), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Select(p => p.ChronicDiseaseCount)
            .Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Handle_ShouldSortByChronicDiseaseCount_Descending()
    {
        await _uow.Patients.AddAsync(MakePatient("Three Diseases", Gender.Male,   "0000001", chronicDiseaseCount: 3));
        await _uow.Patients.AddAsync(MakePatient("No Diseases",    Gender.Female, "0000002", chronicDiseaseCount: 0));
        await _uow.Patients.AddAsync(MakePatient("One Disease",    Gender.Male,   "0000003", chronicDiseaseCount: 1));
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(
            new GetPatientsQuery(null, 1, 10, "chronicDiseaseCount", "desc", null), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Select(p => p.ChronicDiseaseCount)
            .Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoPatients()
    {
        var result = await _handler.Handle(new GetPatientsQuery(null, 1, 10, null, "asc", null), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }
}
