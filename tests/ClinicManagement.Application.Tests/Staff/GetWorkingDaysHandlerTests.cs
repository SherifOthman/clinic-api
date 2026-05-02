using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.Staff.Queries;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using FluentAssertions;

namespace ClinicManagement.Application.Tests.Staff;

public class GetWorkingDaysHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly GetWorkingDaysHandler _handler;

    public GetWorkingDaysHandlerTests()
    {
        _handler = new GetWorkingDaysHandler(_uow);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoDoctorProfileExists()
    {
        var result = await _handler.Handle(new GetWorkingDaysQuery(Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnWorkingDays_WhenDoctorHasDays()
    {
        var spec = TestHandlerHelpers.CreateTestSpecialization();
        await _uow.Specializations.AddAsync(spec);

        var member = TestHandlerHelpers.CreateTestMember();
        await _uow.Members.AddAsync(member);
        await _uow.SaveChangesAsync();

        var doctorInfo = TestHandlerHelpers.CreateTestDoctorInfo(member.Id, spec.Id);
        await _uow.DoctorInfos.AddAsync(doctorInfo);

        var branch = TestHandlerHelpers.CreateTestBranch(clinicId: member.ClinicId);
        await _uow.Branches.AddAsync(branch);
        await _uow.SaveChangesAsync();

        // Create schedule + working days
        var schedule = await _uow.DoctorSchedules.GetOrCreateScheduleAsync(doctorInfo.Id, branch.Id);
        _uow.DoctorSchedules.AddWorkingDay(new WorkingDay
        {
            DoctorBranchScheduleId = schedule.Id,
            Day = DayOfWeek.Monday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 0), IsAvailable = true,
        });
        _uow.DoctorSchedules.AddWorkingDay(new WorkingDay
        {
            DoctorBranchScheduleId = schedule.Id,
            Day = DayOfWeek.Friday, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(14, 0), IsAvailable = false,
        });
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new GetWorkingDaysQuery(member.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.First(d => d.Day == (int)DayOfWeek.Monday).StartTime.Should().Be("09:00");
    }
}
