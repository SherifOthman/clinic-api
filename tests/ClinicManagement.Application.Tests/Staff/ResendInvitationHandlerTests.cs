using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Staff.Commands;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentAssertions;
using Moq;

namespace ClinicManagement.Application.Tests.Staff;

public class ResendInvitationHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IInvitationRepository> _invitationsMock = new();
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly Mock<IClinicRepository> _clinicsMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IEmailService> _emailMock = new();
    private readonly ResendInvitationHandler _handler;

    private readonly Guid _clinicId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public ResendInvitationHandlerTests()
    {
        _uowMock.Setup(u => u.Invitations).Returns(_invitationsMock.Object);
        _uowMock.Setup(u => u.Users).Returns(_usersMock.Object);
        _uowMock.Setup(u => u.Clinics).Returns(_clinicsMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        _currentUserMock.Setup(x => x.GetRequiredClinicId()).Returns(_clinicId);
        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(_userId);

        _usersMock.Setup(x => x.GetByIdAsync(_userId, default))
            .ReturnsAsync(TestHandlerHelpers.CreateTestUser());
        _clinicsMock.Setup(x => x.GetByIdAsync(_clinicId, default))
            .ReturnsAsync(TestHandlerHelpers.CreateTestClinic(ownerUserId: _userId));

        _handler = new ResendInvitationHandler(_uowMock.Object, _currentUserMock.Object, _emailMock.Object);
    }

    private StaffInvitation MakePendingInvitation(Guid clinicId) =>
        StaffInvitation.Create(clinicId, "pending@test.com", ClinicMemberRole.Receptionist, _userId);

    [Fact]
    public async Task Handle_ShouldFail_WhenInvitationNotFound()
    {
        var id = Guid.NewGuid();
        _invitationsMock.Setup(x => x.GetByIdWithSpecializationAsync(id, default))
            .ReturnsAsync((StaffInvitation?)null);

        var result = await _handler.Handle(new ResendInvitationCommand(id), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenInvitationBelongsToDifferentClinic()
    {
        var invitation = MakePendingInvitation(Guid.NewGuid()); // different clinic
        _invitationsMock.Setup(x => x.GetByIdWithSpecializationAsync(invitation.Id, default))
            .ReturnsAsync(invitation);

        var result = await _handler.Handle(new ResendInvitationCommand(invitation.Id), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("FORBIDDEN");
    }

    [Fact]
    public async Task Handle_ShouldSucceed_AndExtendExpiry_WhenInvitationIsPending()
    {
        var invitation = MakePendingInvitation(_clinicId);
        var originalExpiry = invitation.ExpiresAt;
        _invitationsMock.Setup(x => x.GetByIdWithSpecializationAsync(invitation.Id, default))
            .ReturnsAsync(invitation);

        var result = await _handler.Handle(new ResendInvitationCommand(invitation.Id), default);

        result.IsSuccess.Should().BeTrue();
        invitation.ExpiresAt.Should().BeAfter(originalExpiry);
    }

    [Fact]
    public async Task Handle_ShouldSendEmail_WhenSuccessful()
    {
        var invitation = MakePendingInvitation(_clinicId);
        _invitationsMock.Setup(x => x.GetByIdWithSpecializationAsync(invitation.Id, default))
            .ReturnsAsync(invitation);

        await _handler.Handle(new ResendInvitationCommand(invitation.Id), default);

        _emailMock.Verify(e => e.SendStaffInvitationEmailAsync(
            "pending@test.com", It.IsAny<string>(), "Receptionist",
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
