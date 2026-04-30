using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Staff.Commands;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Enums;
using FluentAssertions;
using Moq;

namespace ClinicManagement.Application.Tests.Staff;

/// <summary>
/// Tests for InviteStaffHandler business logic only.
/// Role/specialization validation is tested in InviteStaffValidatorTests.
/// </summary>
public class InviteStaffHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IEmailService> _emailMock = new();
    private readonly InviteStaffHandler _handler;

    public InviteStaffHandlerTests()
    {
        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(Guid.NewGuid());
        _currentUserMock.Setup(x => x.GetRequiredClinicId()).Returns(Guid.NewGuid());
        _handler = new InviteStaffHandler(_uow, _currentUserMock.Object, _emailMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_AndCreateInvitation_ForReceptionist()
    {
        var result = await _handler.Handle(new InviteStaffCommand("Receptionist", "rec@test.com"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().NotBeNullOrEmpty();

        var invitation = await _uow.Invitations.GetByIdAsync(result.Value.InvitationId);
        invitation!.Email.Should().Be("rec@test.com");
        invitation.Role.Should().Be(ClinicMemberRole.Receptionist);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_ForDoctorWithSpecialization()
    {
        // The handler stores the specializationId as-is without validating it exists.
        // Validation (Doctor requires specialization) is enforced by InviteStaffValidator.
        var result = await _handler.Handle(
            new InviteStaffCommand("Doctor", "doc@test.com", Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldSendInvitationEmail()
    {
        await _handler.Handle(new InviteStaffCommand("Receptionist", "rec@test.com"), default);

        _emailMock.Verify(e => e.SendStaffInvitationEmailAsync(
            "rec@test.com", It.IsAny<string>(), "Receptionist",
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
