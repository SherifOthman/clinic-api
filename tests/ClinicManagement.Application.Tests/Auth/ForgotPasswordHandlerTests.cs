using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Features.Auth.Commands.ForgotPassword;
using ClinicManagement.Application.Common.Options;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace ClinicManagement.Application.Tests.Handlers;

public class ForgotPasswordHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<ForgotPasswordHandler>> _loggerMock;
    private readonly ForgotPasswordHandler _handler;

    public ForgotPasswordHandlerTests()
    {
        _context = TestHandlerHelpers.CreateInMemoryContext();
        _userManagerMock = TestHandlerHelpers.CreateMockUserManager();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<ForgotPasswordHandler>>();

        var smtpOptions = Options.Create(new SmtpOptions { FrontendUrl = "https://example.com" });

        _handler = new ForgotPasswordHandler(
            _context,
            _userManagerMock.Object,
            _emailServiceMock.Object,
            smtpOptions,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserDoesNotExist()
    {
        // Arrange
        var email = "nonexistent@test.com";

        // No user in database

        var command = new ForgotPasswordCommand(email);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _emailServiceMock.Verify(
            x => x.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSendEmail_WhenUserExists()
    {
        // Arrange
        var user = TestHandlerHelpers.CreateTestUser();
        user.FirstName = "Test";
        user.LastName = "User";
        user.PasswordHash = "hash123";

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _userManagerMock
            .Setup(x => x.GeneratePasswordResetTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("reset-token-123");

        var command = new ForgotPasswordCommand(user.Email!);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        _emailServiceMock.Verify(
            x => x.SendPasswordResetEmailAsync(
                user.Email!,
                It.IsAny<string>(),
                It.Is<string>(link => link.Contains("reset-token-123")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenEmailServiceThrows()
    {
        // Arrange
        var user = TestHandlerHelpers.CreateTestUser();
        user.FirstName = "Test";
        user.LastName = "User";
        user.PasswordHash = "hash123";

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _userManagerMock
            .Setup(x => x.GeneratePasswordResetTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("token123");

        _emailServiceMock
            .Setup(x => x.SendPasswordResetEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SMTP error"));

        var command = new ForgotPasswordCommand(user.Email!);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _loggerMock.Verify(x => x.Log(LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to send password reset email")),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
