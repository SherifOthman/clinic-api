using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
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
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IEmailService> _emailMock = new();
    private readonly ResendInvitationHandler _handler;

    private readonly Guid _clinicId = Guid.NewGuid();
    private readonly Guid _userId   = Guid.NewGuid();

    public ResendInvitationHandlerTests()
    {
        _currentUserMock.Setup(x => x.GetRequiredClinicId()).Returns(_clinicId);
        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(_userId);
        _handler = new ResendInvitationHandler(_uow, _currentUserMock.Object, _emailMock.Object);
    }

    private async Task<StaffInvitation> SeedPendingInvitationAsync()
    {
        var user = TestHandlerHelpers.CreateTestUser();
        _uow.UserEntities.Add(user);

        var clinic = TestHandlerHelpers.CreateTestClinic(ownerUserId: _userId);
        await _uow.Clinics.AddAsync(clinic);

        var invitation = StaffInvitation.Create(
            clinicId:        _clinicId,
            email:           "pending@test.com",
            role:            ClinicMemberRole.Receptionist,
            createdByUserId: _userId);

        await _uow.Invitations.AddAsync(invitation);
        await _uow.SaveChangesAsync();
        return invitation;
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenInvitationNotFound()
    {
        var result = await _handler.Handle(new ResendInvitationCommand(Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenInvitationBelongsToDifferentClinic()
    {
        var invitation = StaffInvitation.Create(
            clinicId:        Guid.NewGuid(), // different clinic
            email:           "other@test.com",
            role:            ClinicMemberRole.Receptionist,
            createdByUserId: Guid.NewGuid());

        await _uow.Invitations.AddAsync(invitation);
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new ResendInvitationCommand(invitation.Id), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("FORBIDDEN");
    }

    [Fact]
    public async Task Handle_ShouldSucceed_AndExtendExpiry_WhenInvitationIsPending()
    {
        var invitation = await SeedPendingInvitationAsync();
        var originalExpiry = invitation.ExpiresAt;

        var result = await _handler.Handle(new ResendInvitationCommand(invitation.Id), default);

        result.IsSuccess.Should().BeTrue();
        var updated = await _uow.Invitations.GetByIdAsync(invitation.Id);
        updated!.ExpiresAt.Should().BeAfter(originalExpiry);
    }

    [Fact]
    public async Task Handle_ShouldSendEmail_WhenSuccessful()
    {
        var invitation = await SeedPendingInvitationAsync();

        await _handler.Handle(new ResendInvitationCommand(invitation.Id), default);

        _emailMock.Verify(e => e.SendStaffInvitationEmailAsync(
            "pending@test.com", It.IsAny<string>(), "Receptionist",
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
