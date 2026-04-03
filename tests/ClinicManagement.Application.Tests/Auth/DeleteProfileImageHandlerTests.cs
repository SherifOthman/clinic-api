using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Abstractions.Storage;
using ClinicManagement.Application.Features.Auth.Commands;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ClinicManagement.Application.Tests.Handlers;

public class DeleteProfileImageHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<ILogger<DeleteProfileImageHandler>> _loggerMock = new();
    private readonly DeleteProfileImageHandler _handler;

    public DeleteProfileImageHandlerTests()
    {
        _context = TestHandlerHelpers.CreateInMemoryContext();

        _handler = new DeleteProfileImageHandler(
            _context,
            _currentUserServiceMock.Object,
            _fileStorageServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _currentUserServiceMock.Setup(s => s.GetRequiredUserId()).Returns(userId);

        // No user in database

        // Act
        var result = await _handler.Handle(new DeleteProfileImageCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.USER_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenUserHasNoProfileImage()
    {
        // Arrange
        var user = TestHandlerHelpers.CreateTestUser();
        user.ProfileImageUrl = null;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(s => s.GetRequiredUserId()).Returns(user.Id);

        // Act
        var result = await _handler.Handle(new DeleteProfileImageCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.ProfileImageUrl.Should().BeNull();

        _fileStorageServiceMock.Verify(s => s.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDeleteFileAndClearProfileImage_WhenUserHasProfileImage()
    {
        // Arrange
        var user = TestHandlerHelpers.CreateTestUser();
        user.ProfileImageUrl = "profile.jpg";

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(s => s.GetRequiredUserId()).Returns(user.Id);

        // Act
        var result = await _handler.Handle(new DeleteProfileImageCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.ProfileImageUrl.Should().BeNull();
        
        _fileStorageServiceMock.Verify(s => s.DeleteFileAsync("profile.jpg", It.IsAny<CancellationToken>()), Times.Once);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
