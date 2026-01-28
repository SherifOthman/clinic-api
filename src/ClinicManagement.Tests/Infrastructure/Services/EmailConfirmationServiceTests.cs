using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Options;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Infrastructure.Services;

public class EmailConfirmationServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IEmailSender> _emailSenderMock;
    private readonly Mock<IOptions<SmtpOptions>> _optionsMock;
    private readonly EmailConfirmationService _service;
    private readonly SmtpOptions _smtpOptions;

    public EmailConfirmationServiceTests()
    {
        var store = new Mock<IUserStore<User>>();
        var options = new Mock<IOptions<IdentityOptions>>();
        var passwordHasher = new Mock<IPasswordHasher<User>>();
        var userValidators = new List<IUserValidator<User>>();
        var passwordValidators = new List<IPasswordValidator<User>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<User>>>();
        
        _userManagerMock = new Mock<UserManager<User>>(
            store.Object, 
            options.Object, 
            passwordHasher.Object, 
            userValidators, 
            passwordValidators, 
            keyNormalizer.Object, 
            errors.Object, 
            services.Object, 
            logger.Object);
        _emailSenderMock = new Mock<IEmailSender>();
        _optionsMock = new Mock<IOptions<SmtpOptions>>();
        
        _smtpOptions = new SmtpOptions
        {
            FrontendUrl = "https://example.com"
        };
        
        _optionsMock.Setup(x => x.Value).Returns(_smtpOptions);
        
        _service = new EmailConfirmationService(_userManagerMock.Object, _emailSenderMock.Object, _optionsMock.Object);
    }

    [Fact]
    public async Task SendConfirmationEmailAsync_WhenCalled_ShouldGenerateTokenAndSendEmail()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            FirstName = "John",
            UserName = "test@example.com"
        };
        var token = "generated-token";
        
        _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync(token);
        
        _emailSenderMock.Setup(x => x.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.SendConfirmationEmailAsync(user, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        
        _userManagerMock.Verify(x => x.GenerateEmailConfirmationTokenAsync(user), Times.Once);
        _emailSenderMock.Verify(x => x.SendEmailAsync(
            user.Email,
            "Confirm your email address",
            It.Is<string>(body => body.Contains("John") && body.Contains("confirm-email")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendConfirmationEmailAsync_ShouldIncludeCorrectConfirmationLink()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            FirstName = "John",
            UserName = "test@example.com"
        };
        var token = "test-token";
        
        _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync(token);

        string capturedEmailBody = string.Empty;
        _emailSenderMock.Setup(x => x.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .Callback<string, string, string, CancellationToken>((email, subject, body, ct) => capturedEmailBody = body)
            .Returns(Task.CompletedTask);

        // Act
        await _service.SendConfirmationEmailAsync(user, CancellationToken.None);

        // Assert
        var expectedLink = $"{_smtpOptions.FrontendUrl}/confirm-email?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";
        capturedEmailBody.Should().Contain(expectedLink);
    }

    [Fact]
    public async Task ConfirmEmailAsync_WhenSuccessful_ShouldReturnSuccess()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            UserName = "test@example.com"
        };
        var token = "valid-token";
        
        _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, token))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _service.ConfirmEmailAsync(user, token, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        
        _userManagerMock.Verify(x => x.ConfirmEmailAsync(user, token), Times.Once);
    }

    [Fact]
    public async Task ConfirmEmailAsync_WhenFailed_ShouldReturnFailureWithErrors()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            UserName = "test@example.com"
        };
        var token = "invalid-token";
        
        var identityErrors = new[]
        {
            new IdentityError { Code = "InvalidToken", Description = "Invalid token" }
        };
        
        _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, token))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act
        var result = await _service.ConfirmEmailAsync(user, token, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors!.First().Code.Should().Be("Invalid token");
    }

    [Fact]
    public async Task IsEmailConfirmedAsync_WhenEmailConfirmed_ShouldReturnTrue()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            UserName = "test@example.com"
        };
        
        _userManagerMock.Setup(x => x.IsEmailConfirmedAsync(user))
            .ReturnsAsync(true);

        // Act
        var result = await _service.IsEmailConfirmedAsync(user, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _userManagerMock.Verify(x => x.IsEmailConfirmedAsync(user), Times.Once);
    }

    [Fact]
    public async Task IsEmailConfirmedAsync_WhenEmailNotConfirmed_ShouldReturnFalse()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            UserName = "test@example.com"
        };
        
        _userManagerMock.Setup(x => x.IsEmailConfirmedAsync(user))
            .ReturnsAsync(false);

        // Act
        var result = await _service.IsEmailConfirmedAsync(user, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateEmailConfirmationTokenAsync_ShouldReturnToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            UserName = "test@example.com"
        };
        var expectedToken = "generated-token";
        
        _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync(expectedToken);

        // Act
        var result = await _service.GenerateEmailConfirmationTokenAsync(user, CancellationToken.None);

        // Assert
        result.Should().Be(expectedToken);
        _userManagerMock.Verify(x => x.GenerateEmailConfirmationTokenAsync(user), Times.Once);
    }

    [Theory]
    [InlineData("DuplicateEmail", "Email")]
    [InlineData("InvalidEmail", "Email")]
    [InlineData("DuplicateUserName", "UserName")]
    [InlineData("InvalidUserName", "UserName")]
    [InlineData("PasswordTooShort", "Password")]
    [InlineData("PasswordRequiresDigit", "Password")]
    [InlineData("PasswordRequiresLower", "Password")]
    [InlineData("PasswordRequiresUpper", "Password")]
    [InlineData("PasswordRequiresNonAlphanumeric", "Password")]
    [InlineData("UnknownError", "")]
    public async Task ConfirmEmailAsync_WithDifferentErrorCodes_ShouldMapFieldsCorrectly(string errorCode, string expectedField)
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            UserName = "test@example.com"
        };
        var token = "invalid-token";
        
        var identityErrors = new[]
        {
            new IdentityError { Code = errorCode, Description = "Test error" }
        };
        
        _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, token))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act
        var result = await _service.ConfirmEmailAsync(user, token, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors!.First().Field.Should().Be(expectedField);
        result.Errors.First().Code.Should().Be("Test error");
    }
}