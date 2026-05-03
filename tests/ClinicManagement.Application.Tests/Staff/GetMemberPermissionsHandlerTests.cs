using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Staff.Queries;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentAssertions;
using Moq;

namespace ClinicManagement.Application.Tests.Staff;

public class GetMemberPermissionsHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IClinicMemberRepository> _membersMock = new();
    private readonly Mock<IPermissionRepository> _permissionsMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly GetMemberPermissionsHandler _handler;

    private readonly Guid _clinicId = Guid.NewGuid();

    public GetMemberPermissionsHandlerTests()
    {
        _uowMock.Setup(u => u.Members).Returns(_membersMock.Object);
        _uowMock.Setup(u => u.Permissions).Returns(_permissionsMock.Object);

        _currentUserMock.Setup(x => x.GetRequiredClinicId()).Returns(_clinicId);

        _handler = new GetMemberPermissionsHandler(_uowMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenMemberNotFound()
    {
        var id = Guid.NewGuid();
        _membersMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync((ClinicMember?)null);

        var result = await _handler.Handle(new GetMemberPermissionsQuery(id), default);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenMemberBelongsToDifferentClinic()
    {
        var member = TestHandlerHelpers.CreateTestMember(clinicId: Guid.NewGuid()); // different clinic
        _membersMock.Setup(x => x.GetByIdAsync(member.Id, default)).ReturnsAsync(member);

        var result = await _handler.Handle(new GetMemberPermissionsQuery(member.Id), default);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnPermissions_WhenMemberBelongsToClinic()
    {
        var member = TestHandlerHelpers.CreateTestMember(clinicId: _clinicId);
        _membersMock.Setup(x => x.GetByIdAsync(member.Id, default)).ReturnsAsync(member);
        _permissionsMock.Setup(x => x.GetByMemberIdAsync(member.Id, default))
            .ReturnsAsync([Permission.ViewPatients, Permission.CreatePatient]);

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
        _membersMock.Setup(x => x.GetByIdAsync(member.Id, default)).ReturnsAsync(member);
        _permissionsMock.Setup(x => x.GetByMemberIdAsync(member.Id, default))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new GetMemberPermissionsQuery(member.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().BeEmpty();
    }
}
