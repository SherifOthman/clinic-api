using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Staff.Queries;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Enums;
using FluentAssertions;
using Moq;

namespace ClinicManagement.Application.Tests.Staff;

public class GetMemberPermissionsHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly GetMemberPermissionsHandler _handler;

    private readonly Guid _clinicId = Guid.NewGuid();

    public GetMemberPermissionsHandlerTests()
    {
        _currentUserMock.Setup(x => x.GetRequiredClinicId()).Returns(_clinicId);
        _handler = new GetMemberPermissionsHandler(_uow, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenMemberNotFound()
    {
        var result = await _handler.Handle(new GetMemberPermissionsQuery(Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenMemberBelongsToDifferentClinic()
    {
        var member = TestHandlerHelpers.CreateTestMember(clinicId: Guid.NewGuid()); // different clinic
        await _uow.Members.AddAsync(member);
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new GetMemberPermissionsQuery(member.Id), default);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnPermissions_WhenMemberBelongsToClinic()
    {
        var member = TestHandlerHelpers.CreateTestMember(clinicId: _clinicId);
        await _uow.Members.AddAsync(member);
        await _uow.SaveChangesAsync();

        await _uow.Permissions.SetPermissionsAsync(member.Id,
            [Permission.ViewPatients, Permission.CreatePatient]);
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new GetMemberPermissionsQuery(member.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
        result.Value.Should().Contain(Permission.ViewPatients);
        result.Value.Should().Contain(Permission.CreatePatient);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenMemberHasNoPermissions()
    {
        var member = TestHandlerHelpers.CreateTestMember(clinicId: _clinicId);
        await _uow.Members.AddAsync(member);
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new GetMemberPermissionsQuery(member.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().BeEmpty();
    }
}
