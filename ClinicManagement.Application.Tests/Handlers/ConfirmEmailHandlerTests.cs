using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Auth.Commands.ConfirmEmail;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ClinicManagement.Application.Tests.Handlers;

public class ConfirmEmailHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IEmailTokenService> _emailTokenServiceMock = new();
    private readonly Mock<ILogger<ConfirmEmailHandler>> _loggerMock = new();

    private readonly ConfirmEmailHandler _handler;

    public ConfirmEmailHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _handler = new ConfirmEmailHandler(
            _context,
            _emailTokenServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserDoesNotExists()
    {
        var email = "test@test.com";

        // No user in database

        var command = new ConfirmEmailCommand(email, "token");

        var result = await _handler.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenEmailAlreadyConfirmed()
    {
        var user = new User 
        { 
            Email = "test@test.com",
            UserName = "test@test.com"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _emailTokenServiceMock
            .Setup(x => x.IsEmailConfirmedAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new ConfirmEmailCommand(user.Email, "token");

        var result = await _handler.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTokenIsInvalid()
    {
        var user = new User 
        { 
            Email = "test@test.com",
            UserName = "test@test.com"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _emailTokenServiceMock
            .Setup(x => x.IsEmailConfirmedAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _emailTokenServiceMock
            .Setup(x => x.ConfirmEmailAsync(user, "token", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Invalid token"));

        var command = new ConfirmEmailCommand(user.Email, "token");

        var result = await _handler.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldConfirmEmail_WhenTokenIsValid()
    {
        var user = new User 
        { 
            Email = "test@test.com",
            UserName = "test@test.com"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _emailTokenServiceMock
            .Setup(x => x.IsEmailConfirmedAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _emailTokenServiceMock
            .Setup(x => x.ConfirmEmailAsync(user, "token", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new ConfirmEmailCommand(user.Email, "token");

        var result = await _handler.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
