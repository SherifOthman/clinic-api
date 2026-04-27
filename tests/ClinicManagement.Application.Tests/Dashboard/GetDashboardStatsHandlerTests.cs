using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.Dashboard.Queries;
using ClinicManagement.Application.Tests.Common;
using FluentAssertions;

namespace ClinicManagement.Application.Tests.Dashboard;

public class GetDashboardStatsHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly GetDashboardStatsHandler _handler;

    public GetDashboardStatsHandlerTests()
    {
        _handler = new GetDashboardStatsHandler(_uow);
    }

    [Fact]
    public async Task Handle_ShouldReturnZeroCounts_WhenNoData()
    {
        var result = await _handler.Handle(new GetDashboardStatsQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalPatients.Should().Be(0);
        result.Value.ActiveStaff.Should().Be(0);
        result.Value.PendingInvitations.Should().Be(0);
        result.Value.PatientsThisMonth.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldCountPatients_WhenPatientsExist()
    {
        var clinicId = Guid.NewGuid();
        await _uow.Patients.AddAsync(TestHandlerHelpers.CreateTestPatient(clinicId: clinicId));
        await _uow.Patients.AddAsync(TestHandlerHelpers.CreateTestPatient(patientCode: "0002", clinicId: clinicId));
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new GetDashboardStatsQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalPatients.Should().Be(2);
        result.Value.PatientsThisMonth.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldCountActiveStaff_WhenMembersExist()
    {
        var clinicId = Guid.NewGuid();
        var (_, activeMember) = TestHandlerHelpers.CreateTestMember(clinicId: clinicId);
        var (_, inactiveMember) = TestHandlerHelpers.CreateTestMember(clinicId: clinicId);
        inactiveMember.IsActive = false;

        await _uow.Members.AddAsync(activeMember);
        await _uow.Members.AddAsync(inactiveMember);
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new GetDashboardStatsQuery(), default);

        result.Value!.ActiveStaff.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnNullSubscription_WhenNoSubscriptionExists()
    {
        var result = await _handler.Handle(new GetDashboardStatsQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Subscription.Should().BeNull();
    }
}
