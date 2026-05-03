using ClinicManagement.Application.Abstractions.Data;
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
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly Mock<IEmailTokenService> _emailTokenMock = new();
    private readonly ConfirmEmailHandler _handler;

    public ConfirmEmailHandlerTests()
    {
        _uowMock.Setup(u => u.Users).Returns(_usersMock.Object);

        _handler = new ConfirmEmailHandler(_uowMock.Object, _emailTokenMock.Object, NullLogger<ConfirmEmailHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        _usersMock.Setup(x => x.GetByIdAsync(userId, default)).ReturnsAsync((User?)null);

        var result = await _handler.Handle(new ConfirmEmailCommand(userId, "token"), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenEmailAlreadyConfirmed()
    {
        var user = TestHandlerHelpers.CreateTestUser(emailConfirmed: true);
        _usersMock.Setup(x => x.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _emailTokenMock.Setup(x => x.IsEmailConfirmedAsync(user, default)).ReturnsAsync(true);

        var result = await _handler.Handle(new ConfirmEmailCommand(user.Id, "token"), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTokenIsInvalid()
    {
        var user = TestHandlerHelpers.CreateTestUser(emailConfirmed: false);
        _usersMock.Setup(x => x.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _emailTokenMock.Setup(x => x.IsEmailConfirmedAsync(user, default)).ReturnsAsync(false);
        _emailTokenMock.Setup(x => x.ConfirmEmailAsync(user, "bad-token", default))
            .ThrowsAsync(new InvalidOperationException("Invalid token"));

        var result = await _handler.Handle(new ConfirmEmailCommand(user.Id, "bad-token"), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenTokenIsValid()
    {
        var user = TestHandlerHelpers.CreateTestUser(emailConfirmed: false);
        _usersMock.Setup(x => x.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _emailTokenMock.Setup(x => x.IsEmailConfirmedAsync(user, default)).ReturnsAsync(false);
        _emailTokenMock.Setup(x => x.ConfirmEmailAsync(user, "valid-token", default)).Returns(Task.CompletedTask);

        var result = await _handler.Handle(new ConfirmEmailCommand(user.Id, "valid-token"), default);

        result.IsSuccess.Should().BeTrue();
    }
}
