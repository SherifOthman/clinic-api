using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Auth.Commands.Register;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ClinicManagement.Application.Tests.Auth;

public class RegisterHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<UserManager<User>> _userManagerMock = TestHandlerHelpers.CreateMockUserManager();
    private readonly Mock<IEmailTokenService> _emailTokenMock = new();
    private readonly Mock<IAuditWriter> _auditWriterMock = new();
    private readonly RegisterHandler _handler;

    public RegisterHandlerTests()
    {
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        _handler = new RegisterHandler(
            _uowMock.Object, _userManagerMock.Object, _emailTokenMock.Object,
            _auditWriterMock.Object, NullLogger<RegisterHandler>.Instance);
    }

    private RegisterCommand ValidCommand(string email = "new@test.com") =>
        new(email, "newuser", "Test@1234!", "+966500000001", "Male", "New User");

    [Fact]
    public async Task Handle_ShouldSucceed_WithValidData()
    {
        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "ClinicOwner"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _handler.Handle(ValidCommand(), default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserManagerFails()
    {
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email already taken" }));

        var result = await _handler.Handle(ValidCommand(), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("USER_CREATION_FAILED");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRoleAssignmentFails()
    {
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "ClinicOwner"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role not found" }));
        _userManagerMock.Setup(x => x.DeleteAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _handler.Handle(ValidCommand(), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("ROLE_ASSIGNMENT_FAILED");
    }

    [Fact]
    public async Task Handle_ShouldStillSucceed_WhenEmailSendingFails()
    {
        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "ClinicOwner"))
            .ReturnsAsync(IdentityResult.Success);
        _emailTokenMock.Setup(x => x.SendConfirmationEmailAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SMTP error"));

        var result = await _handler.Handle(ValidCommand(), default);

        // Email failure is non-fatal — user is still created
        result.IsSuccess.Should().BeTrue();
    }
}
