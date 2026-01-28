using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Options;
using ClinicManagement.Infrastructure.Common.Interfaces;
using ClinicManagement.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Infrastructure.Services;

public class SmtpEmailSenderTests
{
    private readonly Mock<IEmailSmtpClient> _smtpClientMock;
    private readonly Mock<ILogger<SmtpEmailSender>> _loggerMock;
    private readonly SmtpEmailSender _emailSender;
    private readonly SmtpOptions _smtpOptions;

    public SmtpEmailSenderTests()
    {
        _smtpClientMock = new Mock<IEmailSmtpClient>();
        _loggerMock = new Mock<ILogger<SmtpEmailSender>>();
        
        _smtpOptions = new SmtpOptions
        {
            Host = "smtp.test.com",
            Port = 587,
            UserName = "test@test.com",
            Password = "password",
            FromEmail = "noreply@test.com",
            FromName = "Test App",
            FrontendUrl = "https://app.test.com"
        };

        var optionsMock = new Mock<IOptions<SmtpOptions>>();
        optionsMock.Setup(x => x.Value).Returns(_smtpOptions);

        _emailSender = new SmtpEmailSender(optionsMock.Object, _smtpClientMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SendEmailAsync_WhenValidParameters_ShouldSendEmailSuccessfully()
    {
        // Arrange
        var toEmail = "recipient@test.com";
        var subject = "Test Subject";
        var htmlMessage = "<h1>Test Message</h1>";

        // Act
        await _emailSender.SendEmailAsync(toEmail, subject, htmlMessage);

        // Assert
        _smtpClientMock.Verify(x => x.ConnectAsync(
            _smtpOptions.Host, 
            _smtpOptions.Port, 
            MailKit.Security.SecureSocketOptions.StartTls, 
            It.IsAny<CancellationToken>()), Times.Once);

        _smtpClientMock.Verify(x => x.AuthenticateAsync(
            _smtpOptions.UserName, 
            _smtpOptions.Password, 
            It.IsAny<CancellationToken>()), Times.Once);

        _smtpClientMock.Verify(x => x.SendAsync(
            It.Is<MimeMessage>(m => 
                m.To.ToString().Contains(toEmail) && 
                m.Subject == subject &&
                m.From.ToString().Contains(_smtpOptions.FromEmail)), 
            It.IsAny<CancellationToken>()), Times.Once);

        _smtpClientMock.Verify(x => x.DisconnectAsync(true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WhenFrontendUrlPlaceholder_ShouldReplaceWithActualUrl()
    {
        // Arrange
        var toEmail = "recipient@test.com";
        var subject = "Test Subject";
        var htmlMessage = "<a href='{{FRONTEND_URL}}/verify'>Click here</a>";
        var expectedMessage = $"<a href='{_smtpOptions.FrontendUrl}/verify'>Click here</a>";

        // Act
        await _emailSender.SendEmailAsync(toEmail, subject, htmlMessage);

        // Assert
        _smtpClientMock.Verify(x => x.SendAsync(
            It.Is<MimeMessage>(m => m.HtmlBody.Contains(_smtpOptions.FrontendUrl)), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WhenSmtpClientThrowsException_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var toEmail = "recipient@test.com";
        var subject = "Test Subject";
        var htmlMessage = "<h1>Test Message</h1>";

        _smtpClientMock.Setup(x => x.ConnectAsync(
            It.IsAny<string>(), 
            It.IsAny<int>(), 
            It.IsAny<MailKit.Security.SecureSocketOptions>(), 
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SMTP connection failed"));

        // Act & Assert
        var exception = await FluentActions.Invoking(() => _emailSender.SendEmailAsync(toEmail, subject, htmlMessage))
            .Should().ThrowAsync<InvalidOperationException>();

        exception.Which.Message.Should().Contain("Failed to send email");
        exception.Which.Message.Should().Contain("SMTP connection failed");
    }

    [Fact]
    public async Task SendEmailAsync_WhenAuthenticationFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var toEmail = "recipient@test.com";
        var subject = "Test Subject";
        var htmlMessage = "<h1>Test Message</h1>";

        _smtpClientMock.Setup(x => x.AuthenticateAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Authentication failed"));

        // Act & Assert
        var exception = await FluentActions.Invoking(() => _emailSender.SendEmailAsync(toEmail, subject, htmlMessage))
            .Should().ThrowAsync<InvalidOperationException>();

        exception.Which.Message.Should().Contain("Failed to send email");
        exception.Which.Message.Should().Contain("Authentication failed");
    }

    [Fact]
    public async Task SendEmailAsync_WhenSendFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var toEmail = "recipient@test.com";
        var subject = "Test Subject";
        var htmlMessage = "<h1>Test Message</h1>";

        _smtpClientMock.Setup(x => x.SendAsync(
            It.IsAny<MimeMessage>(), 
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Send failed"));

        // Act & Assert
        var exception = await FluentActions.Invoking(() => _emailSender.SendEmailAsync(toEmail, subject, htmlMessage))
            .Should().ThrowAsync<InvalidOperationException>();

        exception.Which.Message.Should().Contain("Failed to send email");
        exception.Which.Message.Should().Contain("Send failed");
    }

    [Fact]
    public async Task SendEmailAsync_WhenCalled_ShouldLogInformationMessages()
    {
        // Arrange
        var toEmail = "recipient@test.com";
        var subject = "Test Subject";
        var htmlMessage = "<h1>Test Message</h1>";

        // Act
        await _emailSender.SendEmailAsync(toEmail, subject, htmlMessage);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Sending email to")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Email sent successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WhenExceptionOccurs_ShouldLogError()
    {
        // Arrange
        var toEmail = "recipient@test.com";
        var subject = "Test Subject";
        var htmlMessage = "<h1>Test Message</h1>";

        _smtpClientMock.Setup(x => x.ConnectAsync(
            It.IsAny<string>(), 
            It.IsAny<int>(), 
            It.IsAny<MailKit.Security.SecureSocketOptions>(), 
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Connection failed"));

        // Act & Assert
        await FluentActions.Invoking(() => _emailSender.SendEmailAsync(toEmail, subject, htmlMessage))
            .Should().ThrowAsync<InvalidOperationException>();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to send email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}