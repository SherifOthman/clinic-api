using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Staff.Commands;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentAssertions;
using Moq;

namespace ClinicManagement.Application.Tests.Staff;

public class CancelInvitationHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IInvitationRepository> _invitationsMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly CancelInvitationHandler _handler;

    public CancelInvitationHandlerTests()
    {
        _uowMock.Setup(u => u.Invitations).Returns(_invitationsMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        _handler = new CancelInvitationHandler(_uowMock.Object, _currentUserMock.Object);
    }

    private static StaffInvitation MakeInvitation(Guid clinicId) =>
        StaffInvitation.Create(clinicId, "doctor@test.com", ClinicMemberRole.Doctor, Guid.NewGuid());

    [Fact]
    public async Task Handle_ShouldFail_WhenInvitationNotFound()
    {
        var id = Guid.NewGuid();
        _currentUserMock.Setup(x => x.GetRequiredClinicId()).Returns(Guid.NewGuid());
        _invitationsMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync((StaffInvitation?)null);

        var result = await _handler.Handle(new CancelInvitationCommand(id), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenInvitationBelongsToDifferentClinic()
    {
        var invitation = MakeInvitation(Guid.NewGuid());
        _invitationsMock.Setup(x => x.GetByIdAsync(invitation.Id, default)).ReturnsAsync(invitation);
        _currentUserMock.Setup(x => x.GetRequiredClinicId()).Returns(Guid.NewGuid()); // different clinic

        var result = await _handler.Handle(new CancelInvitationCommand(invitation.Id), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldCancel_WhenValid()
    {
        var clinicId = Guid.NewGuid();
        var invitation = MakeInvitation(clinicId);
        _invitationsMock.Setup(x => x.GetByIdAsync(invitation.Id, default)).ReturnsAsync(invitation);
        _currentUserMock.Setup(x => x.GetRequiredClinicId()).Returns(clinicId);

        var result = await _handler.Handle(new CancelInvitationCommand(invitation.Id), default);

        result.IsSuccess.Should().BeTrue();
        invitation.IsCanceled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenAlreadyCanceled()
    {
        var clinicId = Guid.NewGuid();
        var invitation = MakeInvitation(clinicId);
        invitation.Cancel();
        _invitationsMock.Setup(x => x.GetByIdAsync(invitation.Id, default)).ReturnsAsync(invitation);
        _currentUserMock.Setup(x => x.GetRequiredClinicId()).Returns(clinicId);

        var result = await _handler.Handle(new CancelInvitationCommand(invitation.Id), default);

        result.IsSuccess.Should().BeFalse();
    }
}
