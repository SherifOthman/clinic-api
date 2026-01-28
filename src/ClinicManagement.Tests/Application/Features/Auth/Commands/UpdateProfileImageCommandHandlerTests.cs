using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Auth.Commands.UpdateProfileImage;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Auth.Commands;

public class UpdateProfileImageCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly Mock<ILogger<UpdateProfileImageCommandHandler>> _loggerMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly UpdateProfileImageCommandHandler _handler;

    public UpdateProfileImageCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _loggerMock = new Mock<ILogger<UpdateProfileImageCommandHandler>>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _unitOfWorkMock.Setup(x => x.Users).Returns(_userRepositoryMock.Object);

        _handler = new UpdateProfileImageCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _fileStorageServiceMock.Object,
            _dateTimeProviderMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateProfileImageCommand { Image = CreateMockFormFile() };
        _currentUserServiceMock.Setup(x => x.TryGetUserId(out It.Ref<int>.IsAny))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("User not authenticated");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = 1;
        var command = new UpdateProfileImageCommand { Image = CreateMockFormFile() };
        
        _currentUserServiceMock.Setup(x => x.TryGetUserId(out It.Ref<int>.IsAny))
            .Callback(new TryGetUserIdCallback((out int id) => id = userId))
            .Returns(true);
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be(ApplicationErrors.Authentication.USER_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_WhenFileUploadFails_ShouldReturnFailure()
    {
        // Arrange
        var userId = 1;
        var user = new User { Id = userId, FirstName = "Test", LastName = "User" };
        var command = new UpdateProfileImageCommand { Image = CreateMockFormFile() };
        
        _currentUserServiceMock.Setup(x => x.TryGetUserId(out It.Ref<int>.IsAny))
            .Callback(new TryGetUserIdCallback((out int id) => id = userId))
            .Returns(true);
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _fileStorageServiceMock.Setup(x => x.UploadFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FileUploadResult>.Fail("Upload failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("Upload failed");
    }

    [Fact]
    public async Task Handle_WhenSuccessful_ShouldUpdateUserProfileImage()
    {
        // Arrange
        var userId = 1;
        var user = new User { Id = userId, FirstName = "Test", LastName = "User" };
        var command = new UpdateProfileImageCommand { Image = CreateMockFormFile() };
        var now = DateTime.UtcNow;
        var uploadResult = new FileUploadResult
        {
            FileUrl = "/uploads/profile-images/test.png",
            FileName = "test.png",
            FilePath = "profile-images/test.png",
            FileSize = 1024,
            ContentType = "image/png"
        };

        _currentUserServiceMock.Setup(x => x.TryGetUserId(out It.Ref<int>.IsAny))
            .Callback(new TryGetUserIdCallback((out int id) => id = userId))
            .Returns(true);
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _fileStorageServiceMock.Setup(x => x.UploadFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            "profile-images",
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FileUploadResult>.Ok(uploadResult));

        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(now);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ImageUrl.Should().Be(uploadResult.FileUrl);
        result.Value.FileName.Should().Be(uploadResult.FileName);
        result.Value.UpdatedAt.Should().Be(now);

        // Verify user was updated
        user.ProfileImageUrl.Should().Be(uploadResult.FileUrl);
        user.ProfileImageFileName.Should().Be(uploadResult.FileName);
        user.ProfileImageUpdatedAt.Should().Be(now);

        _userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserHasExistingImage_ShouldDeleteOldImage()
    {
        // Arrange
        var userId = 1;
        var user = new User 
        { 
            Id = userId, 
            FirstName = "Test", 
            LastName = "User",
            ProfileImageUrl = "/uploads/profile-images/old-image.png"
        };
        var command = new UpdateProfileImageCommand { Image = CreateMockFormFile() };
        var now = DateTime.UtcNow;
        var uploadResult = new FileUploadResult
        {
            FileUrl = "/uploads/profile-images/new-image.png",
            FileName = "new-image.png",
            FilePath = "profile-images/new-image.png",
            FileSize = 1024,
            ContentType = "image/png"
        };

        _currentUserServiceMock.Setup(x => x.TryGetUserId(out It.Ref<int>.IsAny))
            .Callback(new TryGetUserIdCallback((out int id) => id = userId))
            .Returns(true);
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _fileStorageServiceMock.Setup(x => x.GetFileUrl(It.IsAny<string>()))
            .Returns("/uploads/");

        _fileStorageServiceMock.Setup(x => x.DeleteFileAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _fileStorageServiceMock.Setup(x => x.UploadFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            "profile-images",
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FileUploadResult>.Ok(uploadResult));

        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(now);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        
        // Verify old image was deleted
        _fileStorageServiceMock.Verify(x => x.DeleteFileAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static IFormFile CreateMockFormFile()
    {
        var mock = new Mock<IFormFile>();
        var content = "fake image content";
        var fileName = "test.png";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(content);
        writer.Flush();
        ms.Position = 0;

        mock.Setup(x => x.OpenReadStream()).Returns(ms);
        mock.Setup(x => x.FileName).Returns(fileName);
        mock.Setup(x => x.ContentType).Returns("image/png");
        mock.Setup(x => x.Length).Returns(ms.Length);

        return mock.Object;
    }

    private delegate void TryGetUserIdCallback(out int userId);
}