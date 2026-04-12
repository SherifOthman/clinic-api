using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.Dashboard.Queries;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentAssertions;

namespace ClinicManagement.Application.Tests.Dashboard;

public class GetRecentPatientsHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly GetRecentPatientsHandler _handler;

    public GetRecentPatientsHandlerTests()
    {
        _handler = new GetRecentPatientsHandler(_uow);
    }

    private Patient MakePatient(string name, Gender gender, DateTimeOffset createdAt) => new()
    {
        ClinicId = Guid.NewGuid(), PatientCode = Guid.NewGuid().ToString()[..7],
        FullName = name, DateOfBirth = new DateOnly(1990, 1, 1),
        Gender = gender, CreatedAt = createdAt,
    };

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoPatients()
    {
        var result = await _handler.Handle(new GetRecentPatientsQuery(Count: 5), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldRespectCountLimit()
    {
        var now = DateTimeOffset.UtcNow;
        for (int i = 0; i < 10; i++)
            await _uow.Patients.AddAsync(MakePatient($"Patient {i}", Gender.Male, now.AddMinutes(-i)));
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new GetRecentPatientsQuery(Count: 3), default);

        result.Value.Should().HaveCount(3);
    }

    [Theory]
    [InlineData(Gender.Male, "Male")]
    [InlineData(Gender.Female, "Female")]
    public async Task Handle_ShouldMapGenderToString(Gender gender, string expected)
    {
        await _uow.Patients.AddAsync(MakePatient("Test", gender, DateTimeOffset.UtcNow));
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new GetRecentPatientsQuery(Count: 5), default);

        result.Value.Single().Gender.Should().Be(expected);
    }
}
