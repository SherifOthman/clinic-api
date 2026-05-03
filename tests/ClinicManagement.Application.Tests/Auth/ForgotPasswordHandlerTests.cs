using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Options;
using ClinicManagement.Application.Features.Auth.Commands.ForgotPassword;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace ClinicManagement.Application.Tests.Auth;

public class ForgotPasswordHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly Mock<UserManager<User>> _userManagerMock = TestHandlerHelpers.CreateMockUserManager();
    private readonly Mock<IEmailService> _emailMock = new();
    private readonly Mock<IAuditWriter> _auditWriterMock = new();
    private readonly ForgotPasswordHandler _handler;

    public ForgotPasswordHandlerTests()
    {
        _uowMock.Setup(u => u.Users).Returns(_usersMock.Object);

        _handler = new ForgotPasswordHandler(
            _uowMock.Object, _userManagerMock.Object, _emailMock.Object,
            Options.Create(new AppOptions { FrontendUrl = "https://example.com" }),
            _auditWriterMock.Object, NullLogger<ForgotPasswordHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_AndNotSendEmail_WhenUserDoesNotExist()
    {
        _usersMock.Setup(x => x.GetByEmailOrUsernameAsync("nobody@test.com", default))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(new ForgotPasswordCommand("nobody@test.com"), default);

        result.IsSuccess.Should().BeTrue();
        _emailMock.Verify(x => x.SendPasswordResetEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSendResetEmail_WhenUserExists()
    {
        var user = TestHandlerHelpers.CreateTestUser();
        user.PasswordHash = "hash";
        _usersMock.Setup(x => x.GetByEmailOrUsernameAsync(user.Email!, default)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset-token-123");

        var result = await _handler.Handle(new ForgotPasswordCommand(user.Email!), default);

        result.IsSuccess.Should().BeTrue();
        _emailMock.Verify(x => x.SendPasswordResetEmailAsync(
            user.Email!, It.IsAny<string>(),
            It.Is<string>(link => link.Contains("reset-token-123")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldStillSucceed_WhenEmailServiceFails()
    {
        var user = TestHandlerHelpers.CreateTestUser();
        user.PasswordHash = "hash";
        _usersMock.Setup(x => x.GetByEmailOrUsernameAsync(user.Email!, default)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("token");
        _emailMock.Setup(x => x.SendPasswordResetEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SMTP error"));

        var result = await _handler.Handle(new ForgotPasswordCommand(user.Email!), default);

        result.IsSuccess.Should().BeTrue();
    }
}
