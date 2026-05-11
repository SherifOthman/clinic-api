using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ClinicManagement.Application.Tests.Auth;

public class ConfirmEmailHandlerTests
{
    private readonly Mock<IUserRepository>    _usersMock      = new();
    private readonly Mock<IEmailTokenService> _emailTokenMock = new();
    private readonly ConfirmEmailHandler      _handler;

    public ConfirmEmailHandlerTests()
    {
        _handler = new ConfirmEmailHandler(
            _usersMock.Object, _emailTokenMock.Object,
            NullLogger<ConfirmEmailHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        _usersMock.Setup(x => x.GetByIdAsync(userId, default)).ReturnsAsync((User?)null);

        var result = await _handler.Handle(new ConfirmEmailCommand(userId, "123456"), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("USER_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenEmailAlreadyConfirmed()
    {
        var user = TestHandlerHelpers.CreateTestUser(emailConfirmed: true);
        _usersMock.Setup(x => x.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _emailTokenMock.Setup(x => x.IsEmailConfirmedAsync(user, default)).ReturnsAsync(true);

        var result = await _handler.Handle(new ConfirmEmailCommand(user.Id, "123456"), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("EMAIL_ALREADY_CONFIRMED");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenOtpIsInvalid()
    {
        var user = TestHandlerHelpers.CreateTestUser(emailConfirmed: false);
        _usersMock.Setup(x => x.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _emailTokenMock.Setup(x => x.IsEmailConfirmedAsync(user, default)).ReturnsAsync(false);
        _emailTokenMock.Setup(x => x.VerifyConfirmationOtpAsync(user, "000000", default))
            .ReturnsAsync(false);

        var result = await _handler.Handle(new ConfirmEmailCommand(user.Id, "000000"), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TOKEN_INVALID");
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenOtpIsValid()
    {
        var user = TestHandlerHelpers.CreateTestUser(emailConfirmed: false);
        _usersMock.Setup(x => x.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _emailTokenMock.Setup(x => x.IsEmailConfirmedAsync(user, default)).ReturnsAsync(false);
        _emailTokenMock.Setup(x => x.VerifyConfirmationOtpAsync(user, "123456", default))
            .ReturnsAsync(true);

        var result = await _handler.Handle(new ConfirmEmailCommand(user.Id, "123456"), default);

        result.IsSuccess.Should().BeTrue();
    }
}
