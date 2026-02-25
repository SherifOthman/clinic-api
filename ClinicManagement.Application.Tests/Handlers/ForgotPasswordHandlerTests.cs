using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Auth.Commands.ForgotPassword;
using ClinicManagement.Application.Common.Options;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace ClinicManagement.Application.Tests.Handlers;

public class ForgotPasswordHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailTokenService> _tokenServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<ForgotPasswordHandler>> _loggerMock;
    private readonly ForgotPasswordHandler _handler;

    public ForgotPasswordHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tokenServiceMock = new Mock<IEmailTokenService>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<ForgotPasswordHandler>>();

        var smtpOptions = Options.Create(new SmtpOptions { FrontendUrl = "https://example.com" });

        _handler = new ForgotPasswordHandler(
            _unitOfWorkMock.Object,
            _tokenServiceMock.Object,
            _emailServiceMock.Object,
            smtpOptions,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserDoesNotExist()
    {
        // Arrange
        var email = "nonexistent@test.com";
        _unitOfWorkMock
            .Setup(x => x.Users.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

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
        var user = new User { Email = "test@test.com", PasswordHash = "hash123" };

        _unitOfWorkMock
            .Setup(x => x.Users.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _tokenServiceMock
            .Setup(x => x.GeneratePasswordResetToken(user.Id, user.Email, user.PasswordHash))
            .Returns("reset-token-123");

        var command = new ForgotPasswordCommand(user.Email);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        _emailServiceMock.Verify(
            x => x.SendPasswordResetEmailAsync(
                user.Email,
                It.IsAny<string>(),
                It.Is<string>(link => link.Contains("reset-token-123")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenEmailServiceThrows()
    {
        // Arrange
        var user = new User { Email = "test@test.com", PasswordHash = "hash123" };

        _unitOfWorkMock
            .Setup(x => x.Users.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _tokenServiceMock
            .Setup(x => x.GeneratePasswordResetToken(user.Id, user.Email, user.PasswordHash))
            .Returns("token123");

        _emailServiceMock
            .Setup(x => x.SendPasswordResetEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SMTP error"));

        var command = new ForgotPasswordCommand(user.Email);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
