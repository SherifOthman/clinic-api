using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Auth.Commands.ChangePassword;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ClinicManagement.Application.Tests.Handlers;

public class ChangePasswordHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<ILogger<ChangePasswordHandler>> _loggerMock = new();

    private readonly ChangePasswordHandler _handler;

    public ChangePasswordHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _handler = new ChangePasswordHandler(
            _context,
            _passwordHasherMock.Object,
            _currentUserMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserDoesNotExist()
    {
        // Arrange 
        var userId = Guid.NewGuid();
        _currentUserMock
            .Setup(x => x.GetRequiredUserId())
            .Returns(userId);

        // No user in database

        var command = new ChangePasswordCommand("old", "new");

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenCurrentPasswordIsIncorrect()
    {
        // Arrange
        var user = new User 
        { 
            Email = "test@test.com",
            UserName = "test@test.com"
        };
        user.PasswordHash = "hash";

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _currentUserMock
            .Setup(x => x.GetRequiredUserId())
            .Returns(user.Id);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword("wrong", "hash"))
            .Returns(false);

        var command = new ChangePasswordCommand("wrong", "new");

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenCurrentPasswordIsCorrect()
    {
        // Arrange
        var user = new User 
        { 
            Email = "test@test.com",
            UserName = "test@test.com"
        };
        user.PasswordHash = "oldhash";

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _currentUserMock
            .Setup(x => x.GetRequiredUserId())
            .Returns(user.Id);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword("old", "oldhash"))
            .Returns(true);

        _passwordHasherMock
            .Setup(x => x.HashPassword("new"))
            .Returns("newhash");

        var command = new ChangePasswordCommand("old", "new");

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify in database
        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.PasswordHash.Should().Be("newhash");
        updatedUser.LastPasswordChangeAt.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}