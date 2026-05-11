using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Auth.Commands.ForgotPassword;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ClinicManagement.Application.Tests.Auth;

public class ForgotPasswordHandlerTests
{
    private readonly Mock<IUserRepository>    _usersMock         = new();
    private readonly Mock<IEmailTokenService> _emailTokenMock    = new();
    private readonly Mock<IAuditWriter>       _auditWriterMock   = new();
    private readonly ForgotPasswordHandler    _handler;

    public ForgotPasswordHandlerTests()
    {
        _handler = new ForgotPasswordHandler(
            _usersMock.Object,
            _emailTokenMock.Object,
            _auditWriterMock.Object,
            NullLogger<ForgotPasswordHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_AndNotSendOtp_WhenUserDoesNotExist()
    {
        _usersMock.Setup(x => x.GetByEmailOrUsernameAsync("nobody@test.com", default))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(new ForgotPasswordCommand("nobody@test.com"), default);

        result.IsSuccess.Should().BeTrue();
        _emailTokenMock.Verify(x => x.SendPasswordResetOtpAsync(
            It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSendOtp_WhenUserExists()
    {
        var user = TestHandlerHelpers.CreateTestUser();
        _usersMock.Setup(x => x.GetByEmailOrUsernameAsync(user.Email!, default)).ReturnsAsync(user);
        _emailTokenMock.Setup(x => x.SendPasswordResetOtpAsync(user, default)).Returns(Task.CompletedTask);

        var result = await _handler.Handle(new ForgotPasswordCommand(user.Email!), default);

        result.IsSuccess.Should().BeTrue();
        _emailTokenMock.Verify(x => x.SendPasswordResetOtpAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldStillSucceed_WhenOtpSendingFails()
    {
        var user = TestHandlerHelpers.CreateTestUser();
        _usersMock.Setup(x => x.GetByEmailOrUsernameAsync(user.Email!, default)).ReturnsAsync(user);
        _emailTokenMock.Setup(x => x.SendPasswordResetOtpAsync(user, default))
            .ThrowsAsync(new Exception("SMTP error"));

        var result = await _handler.Handle(new ForgotPasswordCommand(user.Email!), default);

        result.IsSuccess.Should().BeTrue();
    }
}
