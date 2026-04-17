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

    private async Task<ClinicMember> SeedMemberAsync(
        string firstName = "Ahmed", string lastName = "Ali", Gender gender = Gender.Male)
    {
        var (_, member) = TestHandlerHelpers.CreateTestMember(
            firstName: firstName, lastName: lastName, gender: gender,
            role: ClinicMemberRole.Receptionist);
        await _uow.Members.AddAsync(member);
        await _uow.SaveChangesAsync();
        return member;
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
        var member = await SeedMemberAsync("Ahmed", "Ali", Gender.Male);

        var result = await _handler.Handle(new GetStaffDetailQuery(member.Id), default);

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

        var (_, member) = TestHandlerHelpers.CreateTestMember(
            firstName: "Sara", lastName: "Doctor", gender: Gender.Female,
            role: ClinicMemberRole.Doctor);
        await _uow.Members.AddAsync(member);
        await _uow.SaveChangesAsync();

        await _uow.DoctorInfos.AddAsync(TestHandlerHelpers.CreateTestDoctorInfo(member.Id, spec.Id));
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new GetStaffDetailQuery(member.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.DoctorProfile.Should().NotBeNull();
        result.Value.DoctorProfile!.SpecializationNameEn.Should().Be("Cardiology");
    }
}
