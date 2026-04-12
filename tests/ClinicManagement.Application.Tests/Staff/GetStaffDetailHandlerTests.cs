using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.Staff.Queries;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentAssertions;

namespace ClinicManagement.Application.Tests.Staff;

public class GetStaffDetailHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly GetStaffDetailHandler _handler;

    public GetStaffDetailHandlerTests()
    {
        _handler = new GetStaffDetailHandler(_uow);
    }

    private async Task<Domain.Entities.Staff> SeedStaffAsync(
        string firstName = "Ahmed", string lastName = "Ali", Gender gender = Gender.Male)
    {
        var user = new User
        {
            FirstName = firstName, LastName = lastName,
            UserName = $"{firstName.ToLower()}.{lastName.ToLower()}",
            Email = $"{firstName.ToLower()}@test.com",
            PhoneNumber = "+966500000001", Gender = gender,
        };
        _uow.UserEntities.Add(user);
        var staff = new Domain.Entities.Staff { UserId = user.Id, ClinicId = Guid.NewGuid(), IsActive = true };
        await _uow.Staff.AddAsync(staff);
        await _uow.SaveChangesAsync();
        return staff;
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenStaffNotFound()
    {
        var result = await _handler.Handle(new GetStaffDetailQuery(Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnStaffDetail()
    {
        var staff = await SeedStaffAsync("Ahmed", "Ali", Gender.Male);

        var result = await _handler.Handle(new GetStaffDetailQuery(staff.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.FullName.Should().Be("Ahmed Ali");
        result.Value.Gender.Should().Be("Male");
        result.Value.IsActive.Should().BeTrue();
        result.Value.DoctorProfile.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldIncludeDoctorProfile_WhenExists()
    {
        var spec = TestHandlerHelpers.CreateTestSpecialization("Cardiology");
        await _uow.Specializations.AddAsync(spec);
        await _uow.SaveChangesAsync();

        var staff = await SeedStaffAsync("Sara", "Doctor", Gender.Female);
        await _uow.DoctorProfiles.AddAsync(TestHandlerHelpers.CreateTestDoctorProfile(staffId: staff.Id, specializationId: spec.Id));
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new GetStaffDetailQuery(staff.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.DoctorProfile.Should().NotBeNull();
        result.Value.DoctorProfile!.SpecializationNameEn.Should().Be("Cardiology");
    }
}
