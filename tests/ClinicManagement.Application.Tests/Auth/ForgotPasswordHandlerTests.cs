using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
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
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly Mock<UserManager<User>> _userManagerMock = TestHandlerHelpers.CreateMockUserManager();
    private readonly Mock<IEmailService> _emailMock = new();
    private readonly ForgotPasswordHandler _handler;

    public ForgotPasswordHandlerTests()
    {
        _handler = new ForgotPasswordHandler(
            _uow, _userManagerMock.Object, _emailMock.Object,
            Options.Create(new AppOptions { FrontendUrl = "https://example.com" }),
            NullLogger<ForgotPasswordHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_AndNotSendEmail_WhenUserDoesNotExist()
    {
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
        user.FirstName = "Test"; user.LastName = "User"; user.PasswordHash = "hash";
        _uow.UserEntities.Add(user);
        await _uow.SaveChangesAsync();

        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(It.IsAny<User>()))
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
        _uow.UserEntities.Add(user);
        await _uow.SaveChangesAsync();

        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(It.IsAny<User>())).ReturnsAsync("token");
        _emailMock.Setup(x => x.SendPasswordResetEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SMTP error"));

        var result = await _handler.Handle(new ForgotPasswordCommand(user.Email!), default);

        result.IsSuccess.Should().BeTrue();
    }
}
