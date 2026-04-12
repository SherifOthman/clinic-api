using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.Staff.Commands;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using FluentAssertions;

namespace ClinicManagement.Application.Tests.Staff;

public class SaveWorkingDaysHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly SaveWorkingDaysHandler _handler;

    public SaveWorkingDaysHandlerTests()
    {
        _handler = new SaveWorkingDaysHandler(_uow);
    }

    private async Task<(Domain.Entities.Staff staff, DoctorProfile dp)> SeedDoctorAsync()
    {
        var spec  = TestHandlerHelpers.CreateTestSpecialization();
        await _uow.Specializations.AddAsync(spec);
        var staff = TestHandlerHelpers.CreateTestStaff();
        await _uow.Staff.AddAsync(staff);
        await _uow.SaveChangesAsync();

        var dp = TestHandlerHelpers.CreateTestDoctorProfile(staffId: staff.Id, specializationId: spec.Id);
        await _uow.DoctorProfiles.AddAsync(dp);
        await _uow.Branches.AddAsync(new ClinicBranch
        {
            ClinicId = staff.ClinicId, Name = "Main Branch", AddressLine = "123 Test St",
            CountryGeoNameId = 1, StateGeoNameId = 2, CityGeoNameId = 3,
            IsMainBranch = true, IsActive = true,
        });
        await _uow.SaveChangesAsync();
        return (staff, dp);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenDoctorProfileNotFound()
    {
        var result = await _handler.Handle(new SaveWorkingDaysCommand(Guid.NewGuid(), []), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenNoMainBranchExists()
    {
        var spec  = TestHandlerHelpers.CreateTestSpecialization();
        await _uow.Specializations.AddAsync(spec);
        var staff = TestHandlerHelpers.CreateTestStaff();
        await _uow.Staff.AddAsync(staff);
        await _uow.SaveChangesAsync();

        var dp = TestHandlerHelpers.CreateTestDoctorProfile(staffId: staff.Id, specializationId: spec.Id);
        await _uow.DoctorProfiles.AddAsync(dp);
        await _uow.SaveChangesAsync();
        // No branch added

        var result = await _handler.Handle(new SaveWorkingDaysCommand(staff.Id, []), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldSaveWorkingDays()
    {
        var (staff, dp) = await SeedDoctorAsync();

        var result = await _handler.Handle(new SaveWorkingDaysCommand(staff.Id, [
            new(Day: 1, StartTime: "09:00", EndTime: "17:00", IsAvailable: true),
            new(Day: 2, StartTime: "09:00", EndTime: "17:00", IsAvailable: false),
        ]), default);

        result.IsSuccess.Should().BeTrue();

        var days = await _uow.WorkingDays.GetByDoctorProfileIdAsync(dp.Id);
        days.Should().HaveCount(2);
        days.Should().Contain(d => d.Day == (int)DayOfWeek.Monday && d.IsAvailable);
        days.Should().Contain(d => d.Day == (int)DayOfWeek.Tuesday && !d.IsAvailable);
    }

    [Fact]
    public async Task Handle_ShouldReplaceExistingDays_OnSecondSave()
    {
        var (staff, dp) = await SeedDoctorAsync();

        await _handler.Handle(new SaveWorkingDaysCommand(staff.Id, [
            new(1, "09:00", "17:00", true),
            new(2, "09:00", "17:00", true),
            new(3, "09:00", "17:00", true),
        ]), default);

        var result = await _handler.Handle(new SaveWorkingDaysCommand(staff.Id, [
            new(5, "10:00", "14:00", true),
        ]), default);

        result.IsSuccess.Should().BeTrue();

        var days = await _uow.WorkingDays.GetByDoctorProfileIdAsync(dp.Id);
        days.Should().HaveCount(1);
        days.Single().Day.Should().Be((int)DayOfWeek.Friday);
    }
}
