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

public class InviteStaffHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IInvitationRepository> _invitationsMock = new();
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly Mock<IClinicRepository> _clinicsMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IEmailService> _emailMock = new();
    private readonly Mock<IAuditWriter> _auditMock = new();
    private readonly InviteStaffHandler _handler;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _clinicId = Guid.NewGuid();

    public InviteStaffHandlerTests()
    {
        _uowMock.Setup(u => u.Invitations).Returns(_invitationsMock.Object);
        _uowMock.Setup(u => u.Users).Returns(_usersMock.Object);
        _uowMock.Setup(u => u.Clinics).Returns(_clinicsMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        _currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(_userId);
        _currentUserMock.Setup(x => x.GetRequiredClinicId()).Returns(_clinicId);

        // Default: inviter and clinic exist
        _usersMock.Setup(x => x.GetByIdAsync(_userId, default))
            .ReturnsAsync(TestHandlerHelpers.CreateTestUser());
        _clinicsMock.Setup(x => x.GetByIdAsync(_clinicId, default))
            .ReturnsAsync(TestHandlerHelpers.CreateTestClinic(ownerUserId: _userId));

        _handler = new InviteStaffHandler(_uowMock.Object, _currentUserMock.Object, _emailMock.Object, _auditMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_AndCreateInvitation_ForReceptionist()
    {
        StaffInvitation? captured = null;
        _invitationsMock
            .Setup(x => x.AddAsync(It.IsAny<StaffInvitation>(), default))
            .Callback<StaffInvitation, CancellationToken>((inv, _) => captured = inv)
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(new InviteStaffCommand("Receptionist", "rec@test.com"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().NotBeNullOrEmpty();
        captured.Should().NotBeNull();
        captured!.Email.Should().Be("rec@test.com");
        captured.Role.Should().Be(ClinicMemberRole.Receptionist);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_ForDoctorWithSpecialization()
    {
        _invitationsMock.Setup(x => x.AddAsync(It.IsAny<StaffInvitation>(), default)).Returns(Task.CompletedTask);

        var result = await _handler.Handle(
            new InviteStaffCommand("Doctor", "doc@test.com", Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldSendInvitationEmail()
    {
        _invitationsMock.Setup(x => x.AddAsync(It.IsAny<StaffInvitation>(), default)).Returns(Task.CompletedTask);

        await _handler.Handle(new InviteStaffCommand("Receptionist", "rec@test.com"), default);

        _emailMock.Verify(e => e.SendStaffInvitationEmailAsync(
            "rec@test.com", It.IsAny<string>(), "Receptionist",
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
