using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Staff.Commands;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentAssertions;
using Moq;

namespace ClinicManagement.Application.Tests.Staff;

public class CancelInvitationHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly CancelInvitationHandler _handler;

    public CancelInvitationHandlerTests()
    {
        _handler = new CancelInvitationHandler(_uow, _currentUserMock.Object);
    }

    private StaffInvitation MakeInvitation(Guid clinicId) =>
        StaffInvitation.Create(clinicId, "doctor@test.com", ClinicMemberRole.Doctor, Guid.NewGuid());

    [Fact]
    public async Task Handle_ShouldFail_WhenInvitationNotFound()
    {
        _currentUserMock.Setup(x => x.GetRequiredClinicId()).Returns(Guid.NewGuid());

        var result = await _handler.Handle(new CancelInvitationCommand(Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenInvitationBelongsToDifferentClinic()
    {
        var invitation = MakeInvitation(Guid.NewGuid());
        await _uow.Invitations.AddAsync(invitation);
        await _uow.SaveChangesAsync();

        _currentUserMock.Setup(x => x.GetRequiredClinicId()).Returns(Guid.NewGuid()); // different clinic

        var result = await _handler.Handle(new CancelInvitationCommand(invitation.Id), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldCancel_WhenValid()
    {
        var clinicId   = Guid.NewGuid();
        var invitation = MakeInvitation(clinicId);
        await _uow.Invitations.AddAsync(invitation);
        await _uow.SaveChangesAsync();

        _currentUserMock.Setup(x => x.GetRequiredClinicId()).Returns(clinicId);

        var result = await _handler.Handle(new CancelInvitationCommand(invitation.Id), default);

        result.IsSuccess.Should().BeTrue();

        var updated = await _uow.Invitations.GetByIdAsync(invitation.Id);
        updated!.IsCanceled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenAlreadyCanceled()
    {
        var clinicId   = Guid.NewGuid();
        var invitation = MakeInvitation(clinicId);
        invitation.Cancel();
        await _uow.Invitations.AddAsync(invitation);
        await _uow.SaveChangesAsync();

        _currentUserMock.Setup(x => x.GetRequiredClinicId()).Returns(clinicId);

        var result = await _handler.Handle(new CancelInvitationCommand(invitation.Id), default);

        result.IsSuccess.Should().BeFalse();
    }
}
